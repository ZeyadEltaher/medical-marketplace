using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Medical.DAL.Repository;
using Medical.DAL.Constants;
using Medical.DAL.Entities;
using Medical.BLL.Mappers;
using Medical.BLL.Helpers;
using Medical.BLL.Constants;
using Medical.BLL.ViewModels.License;
using Medical.BLL.ResultModels.License;
using Medical.BLL.Infrastructure.Lock.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;
using Medical.BLL.Infrastructure.Cache.Interfaces;

namespace Medical.BLL.Services
{
    public class LicenseServices : ILicenseServices
    {
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly ILicenseRepository _licenseRepo;
        private readonly IProductRepository _productRepo;
        private readonly ILogger<LicenseServices> _logger;
        private readonly ILockProvider _lockProvider;
        private readonly IDistributedCache _cache;
        public LicenseServices(
            IUserProfileRepository userProfileRepo,
            ILicenseRepository licenseRepo,
            IProductRepository productRepo,
            ILogger<LicenseServices> logger,
            ILockProvider lockProvider,
            IDistributedCache cache
        ){
            _userProfileRepo = userProfileRepo;
            _licenseRepo = licenseRepo;
            _productRepo = productRepo;
            _logger = logger;
            _lockProvider = lockProvider;
            _cache = cache;
        }
 
        public async Task<LicenseCacheResult> CacheLicenseAsync(LicenseCreateViewModel model, string path)
        {
            model.ModelId ??= Guid.NewGuid().ToString();

            (string modelKey, string documentKey, string returnUrl, TimeSpan expiry) = LicenseDraftKeys.AllKeys(model.ModelId, path);

            if (model.LicenseDocument is not null)
            {
                // New file uploaded: save its metadata on the model and its bytes in Redis.
                byte[] bytes = await model.LicenseDocument.ConvertToBytesAsync();
                await _cache.StringSetBytesAsync(documentKey, bytes, expiry);
                model.UpdateFileMetaData();
            }

            // IFormFile cannot be serialized to JSON, and ProductsNames is reloaded
            // from the database each time, so clear both before caching the model.
            model.LicenseDocument = null; 
            model.ProductsNames = [];

            // Always re-save the form data (other fields may have changed),
            bool cacheSucceeded = await _cache.StringSetAsync(modelKey, model, expiry);
            if (!cacheSucceeded)
            {
                return LicenseCacheResult.Failure(string.Empty, "Something went wrong while saving your license form. Please try again.");
            }

            // Then refresh the file's expiry so both entries expire together.
            bool refreshSucceeded = await _cache.KeyExpireAsync(documentKey, expiry);
            if (!refreshSucceeded)
            {
                return LicenseCacheResult.Failure(nameof(model.CachedDocumentFileName), "Something went wrong while saving the license document. Please try again.");
            }

            return LicenseCacheResult.Success(returnUrl, model.ModelId);
        }

        public async Task<LicenseRefreshCacheExpiryResult> RefreshCachedLicenseExpiryAsync(LicenseCreateViewModel model)
        {
            TimeSpan expiry = LicenseDraftKeys.Expiry();
            string documentKey = LicenseDraftKeys.Document(model.ModelId);
            byte[]? documentBytes = await _cache.StringGetBytesAsync(documentKey);
            
            // Missing means it expired (10 minutes),
            // It cannot mean that the file was never uploaded,
            // Because this method is never executed unless (LicenseDocumnet is null && CachedDocumentFileName is not null).
            CachedDocumentStatus status = model.CheckCachedDocumentStatus(documentBytes);

            if (status == CachedDocumentStatus.Expired)
            {
                model.RemoveFileMetaData();
                return LicenseRefreshCacheExpiryResult.Failure(nameof(model.CachedDocumentFileName), "The previously uploaded document has expired. Please upload it again.", LicenseFailureReason.InvalidModel);
            }

            // status == CachedDocumentStatus.Exists
            await _cache.KeyExpireAsync(documentKey, expiry);
            return LicenseRefreshCacheExpiryResult.Success();
        }

        public async Task<LicenseRestoreResult> RestoreLicenseAsync(string modelId)
        {
            string modelKey = LicenseDraftKeys.Model(modelId);
            LicenseCreateViewModel? model = await _cache.StringGetAsync<LicenseCreateViewModel>(modelKey);
            return LicenseRestoreResult.Success(model);
        }

        public async Task<List<string>> GetProductNamesAsync()
        {
            return await _productRepo.GetAllNamesAsync();
        }

        public async Task<LicenseOwnerViewModel> GetLicenseOwnerAsync(string licenseId)
        {
            License? license = await _licenseRepo.GetByIdAsync(licenseId);

            if (license == null)
            {
                return null!;
            }

            UserProfile? profile = await _userProfileRepo.GetByUserIdAsync(license.OwnerId);

            if (profile == null)
            {
                return null!; 
            }

            return profile.ToLicenseOwnerViewModel();
        }

        public async Task<List<LicenseListViewModel>> GetAllAsync()
        {
            List<License> licenses = await _licenseRepo.GetAllAsync();

            List<LicenseListViewModel> models = new();

            foreach (License license in licenses)
            {
                LicenseListViewModel model = license.ToLicenseListViewModel();
                models.Add(model);
            }

            return models;
        }

        public async Task<List<LicenseListViewModel>> GetPendingLicensesAsync()
        {
            List<License> licenses = await _licenseRepo.GetPendingAsync();

            List<LicenseListViewModel> models = new();

            foreach (License license in licenses)
            {
                LicenseListViewModel model = license.ToLicenseListViewModel();
                models.Add(model);
            }

            return models;
        }

        public async Task<List<LicenseListViewModel>> GetApprovedLicensesAsync()
        {
            List<License> licenses = await _licenseRepo.GetApprovedAsync();

            List<LicenseListViewModel> models = new();

            foreach (License license in licenses)
            {
                LicenseListViewModel model = license.ToLicenseListViewModel();
                models.Add(model);
            }

            return models;
        }

        public async Task<List<LicenseListViewModel>> GetRejectedLicensesAsync()
        {
            List<License> licenses = await _licenseRepo.GetRejectedAsync();

            List<LicenseListViewModel> models = new();

            foreach (License license in licenses)
            {
                LicenseListViewModel model = license.ToLicenseListViewModel();
                models.Add(model);
            }

            return models;
        }

        public async Task<LicenseGetDetailsModelResult> GetLicenseDetailsModelAsync(string licenseId, string? userId = null, bool isAdmin = false)
        {
            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license == null)
                {
                    return LicenseGetDetailsModelResult.Failure("License wasn't found", LicenseFailureReason.LicenseNotFound);
                }

                bool isOwner = license.OwnerId == userId;
                if (!isOwner && !isAdmin)
                {
                    return LicenseGetDetailsModelResult.Failure("You don't have the right to edit this license", LicenseFailureReason.Unauthorized);
                } 

                LicenseDetailsViewModel model = license.ToLicenseDetailsViewModel();
                return LicenseGetDetailsModelResult.Success(model);
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating LicenseId {LicenseId}", licenseId);
                return LicenseGetDetailsModelResult.Failure("Something went wrong while retrieving the license. Please try again later.");
            }        
        }

        public async Task<LicenseGetEditModelResult> GetLicenseEditModelAsync(string licenseId, string? userId = null, bool isAdmin = false)
        {
            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license == null)
                {
                    return LicenseGetEditModelResult.Failure("License wasn't found", LicenseFailureReason.LicenseNotFound);
                }

                bool isOwner = license.OwnerId == userId;
                if (!isOwner && !isAdmin)
                {
                    return LicenseGetEditModelResult.Failure("You don't have the right to see this license", LicenseFailureReason.Unauthorized);
                } 

                List<string> productNames = await _productRepo.GetAllNamesAsync();
                LicenseEditViewModel model = license.ToLicenseEditViewModel(productNames);
                return LicenseGetEditModelResult.Success(model);      
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating LicenseId {LicenseId}", licenseId);
                return LicenseGetEditModelResult.Failure("Something went wrong while retrieving the license. Please try again later.");
            }     
        }

        public async Task<LicenseCreateResult> CreateAsync(LicenseCreateViewModel model, string ownerId, CancellationToken cancellationToken)
        {
            await using var userLock = await _lockProvider.TryAcquireAsync(
                $"license:user:{ownerId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (userLock is null)
                return LicenseCreateResult.Failure("Another license creation is currently in progress. Please wait.");

            string? fileName = null;

            try
            {
                // Step 1: the product is identified by name from the form, so verify it exists.
                string productName = model.ProductName;
                string? productId = await _productRepo.GetIdByNameAsync(productName);
                if (productId == null)
                {
                    return LicenseCreateResult.Failure(nameof(model.ProductName), "The selected product wasn't found.", LicenseFailureReason.InvalidModel);
                }

                // Step 2: the enum is bound from user input, so it may hold an undefined value.
                bool isValidCategory = Enum.IsDefined(model.LicenseCategory);
                if (!isValidCategory)
                {
                    return LicenseCreateResult.Failure(nameof(model.LicenseCategory), "The selected category wasn't found.", LicenseFailureReason.InvalidModel);
                }

                // We cannot just remove the (if - else) and only use a single (if),
                // Since in such a design of a single if, both path could be executed,
                // This means that if the user was having a cached license then he uploaded a new file.
                // The code will go inside the (if) to write the new file, then get out of (if) to write the cached one. 
                //
                // In fact, this would be a good choice only if the model state prevents a state,
                // Where both properties are not null (LicenseDocument is not null & CachedDocumentFileName is not null),
                // but this will prevent the user from uploading a new file, as long as he had a cached one.

                // Step 3.1 - A new file was posted in this request, so validate and save it.
                if (model.LicenseDocument is not null)
                {
                    fileName = await LicenseDocumentStorage.WriteFileAsync(model.LicenseDocument);
                }

                // Step 3.2 - No new file, so use the one cached in Redis (uploaded before the redirect).
                else
                {
                    // Once we got into this block of code, it means that the user (must have had a cached document),
                    // Since there is a validation in the model that requires at least to achieve 1 of the 2:
                    // 1. Either the user decided to upload a new one (LicenseDocument is not null)
                    // 2. Or he stick to the cached one (CachedDocumentFileName is not null)
                    //
                    // And it is obvious that the first one was not the case (if), 
                    // That's why we are now in the (else).
                    //
                    // Notice we are sure that the user was already having a cached license,
                    // But we are not suer that this cached license is still alive.

                    string documentKey = LicenseDraftKeys.Document(model.ModelId!);
                    byte[]? documentBytes = await _cache.StringGetBytesAsync(documentKey);

                    // Missing means it expired (10 minutes).
                    // It cannot mean that the file was never uploaded,
                    // Because this method is never executed unless:
                    // 1. (LicenseDocumnet is null) guaranteed from skipping the (if) block &&
                    // 2. (CachedDocumentFileName is not null) guaranteed from the model state validation.
                    CachedDocumentStatus status = model.CheckCachedDocumentStatus(documentBytes);

                    if (status == CachedDocumentStatus.Expired)
                    {
                        model.RemoveFileMetaData();
                        return LicenseCreateResult.Failure(nameof(model.CachedDocumentFileName), "The previously uploaded document has expired. Please upload it again.", LicenseFailureReason.InvalidModel);
                    }

                    // The file name is only used to keep the original extension.
                    fileName = await LicenseDocumentStorage.WriteBytesAsync(documentBytes!, model.CachedDocumentFileName!);
                }
                
                // Step 4: save the license with the stored file name.
                License license = model.ToLicense(productId, ownerId, fileName);
                int entries = await _licenseRepo.CreateAsync(license);
                if (entries < 1)
                {
                    await LicenseDocumentStorage.DeleteFileAsync(fileName);
                    return LicenseCreateResult.Failure("Something went wrong while creating the license.");
                }

                return LicenseCreateResult.Success("License created successfully.", license.Id);
            }

            catch (DbUpdateException ex)
            {
                if (fileName is not null)
                {
                    await LicenseDocumentStorage.DeleteFileAsync(fileName);
                }
                _logger.LogError(ex, "Database error while creating License for UserId {UserId}", ownerId);
                return LicenseCreateResult.Failure("Something went wrong while creating the license. Please try again later.");
            }
        }

        public async Task<LicenseEditResult> UpdateAsync(LicenseEditViewModel model, string licenseId, string userId, CancellationToken cancellationToken)
        {
            await using var licenseLock = await _lockProvider.TryAcquireAsync(
                $"license:id:{licenseId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (licenseLock is null)
                return LicenseEditResult.Failure("This license is currently being updated. Please try again.");

            string? oldFileName = null;
            string? newFileName = null;

            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license is null)
                {
                    return LicenseEditResult.Failure("License wasn't found.", LicenseFailureReason.LicenseNotFound);
                }
                
                bool isOwner = license.OwnerId == userId;
                if (!isOwner)
                {
                    return LicenseEditResult.Failure("You don't have the right to update this license", LicenseFailureReason.Unauthorized);
                } 

                string? productId = await _productRepo.GetIdByNameAsync(model.ProductName);
                if (productId is null)
                {
                    return LicenseEditResult.Failure(nameof(model.ProductName) ,"The selected product wasn't found.", LicenseFailureReason.InvalidModel);
                }

                bool isValidCategory = Enum.IsDefined(model.LicenseCategory);
                if (!isValidCategory)
                {
                    return LicenseEditResult.Failure(nameof(model.LicenseCategory), "The selected category wasn't found.", LicenseFailureReason.InvalidModel);
                }

                if (model.NewLicenseDocument is IFormFile file)
                {
                    oldFileName = license.LicenseDocument;
                    newFileName = await LicenseDocumentStorage.WriteFileAsync(file);   
                }

                license.MapExistingLicense(model, productId, newFileName);
                int entries = await _licenseRepo.UpdateAsync(license);
                if (entries < 1)
                {
                    if (newFileName is not null)
                    {
                        await LicenseDocumentStorage.DeleteFileAsync(newFileName);
                    }
                    return LicenseEditResult.Failure("Something happened while updating the license. Please try again later.");
                }

                if (newFileName is not null)
                {
                    await LicenseDocumentStorage.DeleteFileAsync(oldFileName!); 
                }

                return LicenseEditResult.Success("License updated successfully. It has been submitted for admin review.");
            }

            catch (DbUpdateException ex)
            {
                if (newFileName is not null)
                {
                    await LicenseDocumentStorage.DeleteFileAsync(newFileName);
                }
     
                _logger.LogError(ex, "Database error while updating LicenseId {LicenseId}", licenseId);
                return LicenseEditResult.Failure("Something went wrong while updating the license. Please try again later.");
            }
        }

        public async Task<LicenseDeleteResult> DeleteAsync(string licenseId, string userId, CancellationToken cancellationToken)
        {
            await using var licenseLock = await _lockProvider.TryAcquireAsync(
                $"license:id:{licenseId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(15),
                cancellationToken);

            if (licenseLock is null)
                return LicenseDeleteResult.Failure("This license is currently being processed. Please try again.");

            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license is null)
                {
                    return LicenseDeleteResult.Failure("License wasn't found.", LicenseFailureReason.LicenseNotFound);
                }

                bool isOwner = license.OwnerId == userId;
                if (!isOwner)
                {
                    return LicenseDeleteResult.Failure("You don't have the right to delete this license.", LicenseFailureReason.Unauthorized);
                }

                int entries = await _licenseRepo.DeleteByIdAsync(license);
                if (entries < 1)
                {
                    return LicenseDeleteResult.Failure("Something happened while deleting the license. Please try again later.");
                }

                return LicenseDeleteResult.Success("License deleted successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting LicenseId {LicenseId}", licenseId);
                return LicenseDeleteResult.Failure("Something went wrong while deleting the license. Please try again later.");
            }
        }

        public async Task<LicenseApproveResult> ApproveLicenseAsync(string licenseId, CancellationToken cancellationToken)
        {
            await using var licenseLock = await _lockProvider.TryAcquireAsync(
                $"license:id:{licenseId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(15),
                cancellationToken);

            if (licenseLock is null)
                return LicenseApproveResult.Failure("This license status is currently being updated. Please try again.");

            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license is null)
                {
                    return LicenseApproveResult.Failure("License wasn't found.", LicenseFailureReason.LicenseNotFound);
                }

                bool isPending = license.LicenseStatus == LicenseStatus.Pending;
                if (!isPending)
                {
                    return LicenseApproveResult.Failure("This license is not pending.", LicenseFailureReason.LicenseNotPending);
                }

                int entries = await _licenseRepo.ApproveAsync(license);
                if (entries < 1)
                {
                    return LicenseApproveResult.Failure("Something happened while approving the license. Please try again later.");
                }

                return LicenseApproveResult.Success("License approved successfully.");
            }
            
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while approving LicenseId {LicenseId}", licenseId);
                return LicenseApproveResult.Failure("Something went wrong while approving the license. Please try again later.");
            }
        }

        public async Task<LicenseRejectResult> RejectLicenseAsync(string licenseId, CancellationToken cancellationToken)
        {
            await using var licenseLock = await _lockProvider.TryAcquireAsync(
                $"license:id:{licenseId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(15),
                cancellationToken);

            if (licenseLock is null)
                return LicenseRejectResult.Failure("This license status is currently being updated. Please try again.");

            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license is null)
                {
                    return LicenseRejectResult.Failure("License wasn't found.", LicenseFailureReason.LicenseNotFound);
                }

                bool isPending = license.LicenseStatus == DAL.Constants.LicenseStatus.Pending;
                if (!isPending)
                {
                    return LicenseRejectResult.Failure("This license is not pending.", LicenseFailureReason.LicenseNotPending);
                }

                int entries = await _licenseRepo.RejectAsync(license);
                if (entries < 1)
                {
                    return LicenseRejectResult.Failure("Something happened while rejecting the license. Please try again later.");
                }

                return LicenseRejectResult.Success("License rejected successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while rejecting LicenseId {LicenseId}", licenseId);
                return LicenseRejectResult.Failure("Something went wrong while rejecting the license. Please try again later.");
            }
        }
    }
}

