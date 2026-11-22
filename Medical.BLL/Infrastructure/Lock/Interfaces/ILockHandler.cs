using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Interfaces
{
    /// <summary>
    /// Represents a lock that has already been granted. Renews its lease in the background,
    /// reports lock loss through LockLost, and releases the lock on DisposeAsync.
    /// </summary>
    public interface ILockHandler : IAsyncDisposable
    {
        string Resource { get; }
        LockType Type { get; }

        /// <summary>
        /// Cancelled if we LOST the lock while working (lease could not be renewed, Redis down for too long...).
        /// Pass it to your DB / HTTP calls so the work stops instead of running without protection.
        /// </summary>
        CancellationToken LockLost { get; }
    }
}
