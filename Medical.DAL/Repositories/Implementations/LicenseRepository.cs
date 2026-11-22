using Medical.DAL.Context;
using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.DAL.Repository;

public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _context;
    public LicenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(License productLicense)
    {
        await _context.Licenses.AddAsync(productLicense);
        return await _context.SaveChangesAsync();
    }

    private IQueryable<License> LicenseQuery()
    {
        return _context.Licenses
            .Include(license => license.Product)
            .Include(license => license.Owner)
            .Where(license => license.IsDeleted == false);
    }

    public async Task<List<License>> GetAllAsync()
    {
        IQueryable<License> query = LicenseQuery();
        return await query.ToListAsync();
    }

    public async Task<List<License>> GetByOwnerIdAsync(string ownerId)
    {
        IQueryable<License> query = LicenseQuery();
        
        return await query
            .Where(license => license.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<License?> GetByIdAsync(string id)
    {
        IQueryable<License> query = LicenseQuery();

        return await query
            .Where(license => license.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> LicenseExistsForUserAsync(string licenseId, string ownerId)
    {
        return await _context.Licenses.AnyAsync(license =>
            license.Id == licenseId &&
            license.OwnerId == ownerId &&
            license.IsDeleted == false
        );
    }

    public async Task<int> UpdateAsync(License license)
    {
        _context.Update(license);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(License license)
    {
        license.IsDeleted = true;
        license.DeletedAt = DateTime.UtcNow;
        _context.Licenses.Update(license);
        return await _context.SaveChangesAsync();
    }

    public async Task<List<License>> GetPendingAsync()
    {
        IQueryable<License> query = LicenseQuery();

        return await query
        .Where(license => license.LicenseStatus == Constants.LicenseStatus.Pending)
        .ToListAsync();
    }

    public async Task<List<License>> GetApprovedAsync()
    {
        IQueryable<License> query = LicenseQuery();
        
        return await query
        .Where(license => license.LicenseStatus == Constants.LicenseStatus.Approved)
        .ToListAsync();
    }

    public async Task<List<License>> GetRejectedAsync()
    {
        IQueryable<License> query = LicenseQuery();
        
        return await query
        .Where(license => license.LicenseStatus == Constants.LicenseStatus.Rejected)
        .ToListAsync();
    }

    public async Task<int> ApproveAsync(License license)
    {
        license.LicenseStatus = Constants.LicenseStatus.Approved;
        _context.Licenses.Update(license);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> RejectAsync(License license)
    {
        license.LicenseStatus = Constants.LicenseStatus.Rejected;
        _context.Licenses.Update(license);
        return await _context.SaveChangesAsync();
    }
}