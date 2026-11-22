using System.Diagnostics;
using Medical.BLL.Infrastructure.Lock.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;

namespace Medical.BLL.Infrastructure.Lock.Implementations
{
    /// <summary>
    /// Represents a lock that has already been granted. Renews its lease in the background,
    /// reports lock loss through LockLost, and releases the lock on DisposeAsync.
    /// </summary>
    public class RedisLockHandler : ILockHandler
    {
        private readonly ILockCore _core;
        private readonly LockRequest _request;
        private readonly Task _renewLoop;
        
        private int _disposed;
        public LockType Type => _request.Type;
        public string Resource => _request.Resource;
        public CancellationToken LockLost => _lost.Token;
        private readonly CancellationTokenSource _lost = new();
        private readonly CancellationTokenSource _stopRenewing = new();
        
        public RedisLockHandler(ILockCore core, LockRequest request)
        {
            _core = core;
            _request = request;
            _renewLoop = RenewLoopAsync();
        }

        public async Task RenewLoopAsync()
        {
            // Renew roughly every one-third of the lease, leaving room for transient failures.
            double intervalMs = Math.Max(1d, _request.LeaseMs / 3d);
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
            Stopwatch sinceLastSuccess = Stopwatch.StartNew();

            try
            {
                while (await timer.WaitForNextTickAsync(_stopRenewing.Token).ConfigureAwait(false))
                {
                    try
                    {
                        int renewed = await _core.RenewAsync(_request).ConfigureAwait(false);

                        if (renewed != LockScriptResults.Granted)
                        {
                            _lost.Cancel();
                            return;
                        }

                        sinceLastSuccess.Restart();
                    }
                    catch (Exception) when (!_stopRenewing.IsCancellationRequested)
                    {
                        // Retry on the next timer tick. If renewal has failed for a full lease duration,
                        // we can no longer safely claim that this process still owns the lock.
                        if (sinceLastSuccess.ElapsedMilliseconds >= _request.LeaseMs)
                        {
                            _lost.Cancel();
                            return;
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (_stopRenewing.IsCancellationRequested)
            {
                // DisposeAsync intentionally stopped the renewal loop.
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _stopRenewing.Cancel();

            try
            {
                // Ensure no renewal can race with Release.
                try
                {
                    await _renewLoop.ConfigureAwait(false);
                }
                catch
                {
                    // Release must still be attempted if the background task unexpectedly failed.
                }

                await _core.ReleaseAsync(_request).ConfigureAwait(false);
            }
            finally
            {
                _stopRenewing.Dispose();
            }
        }
    }
}