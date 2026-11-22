using Medical.BLL.Infrastructure.Lock.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;

namespace Medical.BLL.Infrastructure.Lock.Implementations
{
    /// <summary>
    /// Public entry point for acquiring distributed reader/writer locks.
    /// Coordinates request creation, the immediate Acquire attempt, and waiting when needed.
    /// Register as Scoped so the scoped ILockOwnerProvider is shared within one request scope.
    /// </summary>
    public class RedisLockProvider : ILockProvider
    {
        public sealed record LockOwner(string Id);

        private readonly ILockCore _core;
        private readonly LockOwner _owner;
        private readonly ILockWaiter _waiter;
        public RedisLockProvider(
            ILockCore core, 
            LockOwner owner,
            ILockWaiter waiter)
        {
            _core = core;
            _owner = owner;
            _waiter = waiter;
        }

        public async Task<ILockHandler?> TryAcquireAsync(string resource, LockType lockType, TimeSpan waitTimeout, TimeSpan leaseTime, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(resource);

            if (waitTimeout < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(waitTimeout), "Wait timeout cannot be negative.");

            if (leaseTime <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(leaseTime), "Lease time must be greater than zero.");

            long leaseMs = (long)leaseTime.TotalMilliseconds;

            if (leaseMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(leaseTime), "Lease time must be at least one millisecond.");

            ct.ThrowIfCancellationRequested();

            var request = new LockRequest(
                Resource: resource,
                Type: lockType,
                Mode: lockType == LockType.Shared ? "S" : "X",
                OwnerId: _owner.Id,
                LeaseMs: leaseMs,
                Channel: LockKeys.Channel(resource),
                Keys:
                [
                    LockKeys.Owner(resource),
                    LockKeys.Queue(resource),
                    LockKeys.Heartbeat(resource),
                    LockKeys.Meta(resource)
                ]);

            DateTime deadlineUtc = DateTime.UtcNow + waitTimeout;

            // Fast path: one atomic Redis script either grants immediately or queues the request.
            // If this script throws, we deliberately do not Cancel: we cannot know whether the request
            // was reentrant, and Cancel could release the caller's already-held outer lock.
            int result = await _core.AcquireAsync(request).ConfigureAwait(false);

            if (result == LockScriptResults.UpgradeNotSupported)
                throw new LockUpgradeNotSupportedException(resource);

            if (result == LockScriptResults.Granted)
                return new RedisLockHandler(_core, request);

            if (result != LockScriptResults.StillWaiting)
                throw new InvalidOperationException($"Unexpected Acquire result: {result}.");

            bool acquired = await _waiter
                .WaitAsync(request, deadlineUtc, ct)
                .ConfigureAwait(false);

            return acquired ? new RedisLockHandler(_core, request) : null;
        }
    }
}
