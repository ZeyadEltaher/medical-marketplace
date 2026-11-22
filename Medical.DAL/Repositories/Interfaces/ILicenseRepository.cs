using Medical.DAL.Entities;

namespace Medical.DAL.Repository;

public interface ILicenseRepository
{
    Task<List<License>> GetAllAsync();
    Task<License?> GetByIdAsync(string id);
    Task<List<License>> GetByOwnerIdAsync(string ownerId);
    Task<List<License>> GetPendingAsync();
    Task<List<License>> GetApprovedAsync();
    Task<List<License>> GetRejectedAsync();
    Task<bool> LicenseExistsForUserAsync(string licenseId, string ownerId);
    Task<int> CreateAsync(License productLicense);
    Task<int> UpdateAsync(License license);
    Task<int> DeleteByIdAsync(License license);
    Task<int> ApproveAsync(License license);
    Task<int> RejectAsync(License license);
}