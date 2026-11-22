namespace Medical.BLL.Infrastructure.Cache.Interfaces
{
    public interface IDistributedCache
    {
        Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiry);
        Task<T?> StringGetAsync<T>(string key);
        Task<bool> StringSetBytesAsync(string key, byte[] bytes, TimeSpan expiry);
        Task<byte[]?> StringGetBytesAsync(string key);
        Task<bool> KeyExpireAsync(string key, TimeSpan expiry);
        Task<bool> DeleteAsync(string key);
    }
}