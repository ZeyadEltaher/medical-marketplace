using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Interfaces
{
    public interface ILockProvider
    {
        /// <summary>
        /// Waits in the FIFO queue until the lock is granted, or returns null after <paramref name="waitTimeout"/>.
        /// </summary>
        /// <param name="resource">What we lock, e.g. "patient:42".</param>
        /// <param name="leaseTime">Lock lifetime if the holder dies. It is renewed automatically while the handle is alive.</param>
        Task<ILockHandler?> TryAcquireAsync(string resource, LockType lockType, TimeSpan waitTimeout, TimeSpan leaseTime, CancellationToken ct = default);
    }

    /// <summary>Thrown when the caller holds an S-lock and asks for an X-lock on the same resource.</summary>
    public sealed class LockUpgradeNotSupportedException : InvalidOperationException
    {
        public LockUpgradeNotSupportedException(string resource) : base
        ($"Cannot upgrade a Shared lock to Exclusive on '{resource}'. " +
          "Acquire the Exclusive lock first (from the top-level method).") { }
    }
}