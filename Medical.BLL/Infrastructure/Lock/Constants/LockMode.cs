namespace Medical.BLL.Infrastructure.Lock.Constants
{
    public enum LockType
    {
        /// <summary>S-lock: many readers may hold it at the same time.</summary>
        Shared,

        /// <summary>X-lock: exactly one writer, and no readers at the same time.</summary>
        Exclusive
    }
}