using StackExchange.Redis;
using Medical.BLL.Infrastructure.Lock.Interfaces;
using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Implementations
{
    /// <summary>
    /// Owns the waiting workflow: Pub/Sub subscription, deadline checks, fallback ticks,
    /// Poll retries, rejoining after heartbeat eviction, and best-effort cancellation.
    /// It does not execute Lua directly; all Redis commands go through RedisLockCore.
    /// </summary>
    public class RedisLockWaiter : ILockWaiter
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILockCore _core;
        public RedisLockWaiter(
            IConnectionMultiplexer redis, 
            ILockCore core)
        {
            _redis = redis;
            _core = core;
        }

        /// <returns>True if the requesst acquired the lock; false if its wait timeout elapsed.</returns>
        public async Task<bool> WaitAsync(LockRequest request, DateTime deadlineUtc, CancellationToken cancellationToken)
        {
            using var wake = new SemaphoreSlim(initialCount: 0, maxCount: int.MaxValue);

            void onMessage(RedisChannel _1, RedisValue _2)
            {
                try
                {
                    wake.Release();
                }
                catch (ObjectDisposedException)
                {
                    // The callback raced with disposal of the waiter.
                }
                catch (SemaphoreFullException)
                {
                    // A pending wake-up already exists; an extra one is unnecessary.
                }
            }

            RedisChannel channel = RedisChannel.Literal(request.Channel);
            ISubscriber subscriber = _redis.GetSubscriber();
            bool subscribed = false;

            try
            {
                await subscriber.SubscribeAsync(channel, onMessage).ConfigureAwait(false);
                subscribed = true;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Poll immediately after subscribing, so a Pub/Sub message sent between Acquire and
                    // Subscribe cannot cause us to miss a grant.
                    int result = await _core.PollAsync(request).ConfigureAwait(false);

                    if (result == LockScriptResults.Granted)
                        return true;

                    if (result == LockScriptResults.EvictedFromQueue)
                    {
                        // Our heartbeat expired and cleanup removed us. Rejoin with a new ticket.
                        result = await _core.AcquireAsync(request).ConfigureAwait(false);

                        if (result == LockScriptResults.Granted)
                            return true;

                        if (result == LockScriptResults.UpgradeNotSupported)
                            throw new LockUpgradeNotSupportedException(request.Resource);

                        if (result != LockScriptResults.StillWaiting)
                            throw new InvalidOperationException($"Unexpected Acquire result: {result}.");
                    }

                    if (result != LockScriptResults.StillWaiting && result != LockScriptResults.EvictedFromQueue)
                    {
                        throw new InvalidOperationException($"Unexpected Poll result: {result}.");
                    }

                    TimeSpan remaining = deadlineUtc - DateTime.UtcNow;
                    if (remaining <= TimeSpan.Zero)
                    {
                        await CancelQuietlyAsync(request).ConfigureAwait(false);
                        return false;
                    }

                    TimeSpan delay = remaining < LockSettings.WaitTick ? remaining : LockSettings.WaitTick;

                    // Returns false when the tick elapses; returns true when Pub/Sub signals.
                    // Either case starts another loop iteration and another Poll.
                    await wake.WaitAsync(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                // If cancellation/failure races with a grant, Cancel's Lua script removes an unobserved
                // grant too, preventing a lock from being held by a caller that never received a handle.
                await CancelQuietlyAsync(request).ConfigureAwait(false);
                throw;
            }
            finally
            {
                if (subscribed)
                {
                    try
                    {
                        await subscriber.UnsubscribeAsync(channel, onMessage).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Unsubscribe is best-effort; it must not mask the actual result/exception.
                    }
                }
            }
        }

        public async Task CancelQuietlyAsync(LockRequest request)
        {
            try
            {
                await _core.CancelAsync(request).ConfigureAwait(false);
            }
            catch
            {
                // Best effort. If Redis is unavailable, the waiter heartbeat will eventually expire.
            }
        }
    }
}