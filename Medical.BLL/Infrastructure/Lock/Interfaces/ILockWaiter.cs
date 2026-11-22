using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Interfaces
{
    public interface ILockWaiter
    {
        Task<bool> WaitAsync(LockRequest request, DateTime deadlineUtc, CancellationToken cancellationToken);
        Task CancelQuietlyAsync(LockRequest request);
    }
}

