using Medical.DAL.Entities;

namespace Medical.DAL.Repository
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(string userId);
        Task<UserProfile?> GetWithUserByUserIdAsync(string userId);
        Task<UserProfile?> GetByEmailAsync(string email);
        Task<int> CreateAsync(UserProfile userProfile);
        Task<int> UpdateAsync(UserProfile userProfile);
        Task<bool> AnyAsync(string userId);
    }
}