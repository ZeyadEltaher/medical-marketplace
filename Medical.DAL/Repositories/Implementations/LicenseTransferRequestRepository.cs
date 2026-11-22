using Medical.DAL.Constants;
using Medical.DAL.Context;
using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.DAL.Repositories
{
    public class LicenseTransferRequestRepository : ILicenseTransferRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public LicenseTransferRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LicenseTransferRequest?> GetLicensePendingRequestForBuyerAsync(string licenseId, string userId)
        {
            return await _context.LicenseTransferRequests
                .FirstOrDefaultAsync(request =>
                    request.LicenseId == licenseId &&
                    request.BuyerUserId == userId &&
                    request.Status == RequestStatus.Pending
                );
        }

        private IQueryable<LicenseTransferRequest> RequestQuery()
        {
            return _context.LicenseTransferRequests
                .Include(request => request.License)
                .Include(request => request.Seller)
                .Include(request => request.Buyer);
        }

        public async Task<int> RequestAsync(LicenseTransferRequest request)
        {
            await _context.LicenseTransferRequests.AddAsync(request);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> ApproveAsync(LicenseTransferRequest request)
        {
            request.Status = RequestStatus.Approved;
            request.License.OwnerId = request.BuyerUserId;
            foreach (LicenseTransferRequest otherRequest in request.License.LicenseTransferRequests)
            {
                if (otherRequest.Id != request.Id && otherRequest.Status == RequestStatus.Pending)
                {
                    otherRequest.Status = RequestStatus.Cancelled;
                    _context.LicenseTransferRequests.Update(otherRequest);
                }
            }
            _context.LicenseTransferRequests.Update(request);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RejectAsync(LicenseTransferRequest request)
        {
            request.Status = RequestStatus.Rejected;
            _context.LicenseTransferRequests.Update(request);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CancelAsync(LicenseTransferRequest request)
        {
            request.Status = RequestStatus.Cancelled;
            _context.LicenseTransferRequests.Update(request);
            return await _context.SaveChangesAsync();
        }

        public async Task<LicenseTransferRequest?> GetByIdAsync(string requestId)
        {
            IQueryable<LicenseTransferRequest> query = RequestQuery();
                
            return await query
                .Where(request => request.Id == requestId)
                .FirstOrDefaultAsync();
        }
        
        public async Task<LicenseTransferRequest?> GetByIdForApprovalAsync(string requestId)
        {
            return await _context.LicenseTransferRequests
                .Include(request => request.License)
                .ThenInclude(license => license.LicenseTransferRequests)
                .FirstOrDefaultAsync(request => request.Id == requestId);
        }

        public async Task<List<LicenseTransferRequest>> GetSentRequests(string userId, RequestStatus? status = null)
        {
            IQueryable<LicenseTransferRequest> query = RequestQuery();

            if (status == null)
            {
                return await query
                    .Where(request => request.BuyerUserId == userId)
                    .ToListAsync();
            }
    
            return await query
                .Where(request => request.BuyerUserId == userId && request.Status == status)
                .ToListAsync();
        }

        public async Task<List<LicenseTransferRequest>> GetReceivedRequests(string userId, RequestStatus? status = null)
        {
            IQueryable<LicenseTransferRequest> query = RequestQuery();

            if (status == null)
            {            
                return await query
                    .Where(request => request.SellerUserId == userId)
                    .ToListAsync();
            }
    
            return await query
                .Where(request => request.SellerUserId == userId && request.Status == status)
                .ToListAsync();
        }

        public async Task<bool> ExistsForBuyerAsync(string requestId, string userId)
        {
            return await _context.LicenseTransferRequests
            .AnyAsync(request => request.Id == requestId && request.BuyerUserId == userId);
        }

        public async Task<bool> ExistsForSellerAsync(string requestId, string userId)
        {
            return await _context.LicenseTransferRequests
            .AnyAsync(request => request.Id == requestId && request.SellerUserId == userId);
        }
    }
}