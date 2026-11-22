using Medical.DAL.Context;
using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.DAL.Repository
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;
        public UserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByUserIdAsync(string userId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(license => license.UserId == userId);
        }

        public async Task<UserProfile?> GetWithUserByUserIdAsync(string userId)
        {
            return await _context.UserProfiles
                .Include(profile => profile.User)
                .FirstOrDefaultAsync(profile => profile.UserId == userId);
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(license => license.User.Email == email);
        }

        public async Task<int> CreateAsync(UserProfile userProfile)
        {
            await _context.UserProfiles.AddAsync(userProfile);
            return await _context.SaveChangesAsync();
        }


        public async Task<int> UpdateAsync(UserProfile userProfile)
        {
            _context.UserProfiles.Update(userProfile);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(string userId)
        {
            return await _context.UserProfiles.AnyAsync(license => license.UserId == userId);
        }
    }
}