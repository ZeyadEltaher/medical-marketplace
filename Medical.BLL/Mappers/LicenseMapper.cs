using Medical.BLL.ViewModels.License;
using Medical.DAL.Constants;
using Medical.DAL.Entities;

namespace Medical.BLL.Mappers
{
    public static class LicenseMapper
    {
        public static License ToLicense(this LicenseCreateViewModel model, string productId, string ownerId, string fileName)
        {
            return new License
            {
                LicenseCategory = model.LicenseCategory,
                TradeName = model.TradeName!,
                ActiveIngredients = model.ActiveIngredients!,
                DosageForm = model.DosageForm!,
                RegistrationNumber = model.RegistrationNumber!,
                IssueDate = model.IssueDate,
                ExpiryDate = model.ExpiryDate,
                CurrentOwnerCompany = model.CurrentOwnerCompany!,
                AskingPrice = model.AskingPrice,
                LicenseDocument = $"/uploads/{fileName}",
                LicenseStatus = LicenseStatus.Pending,
                ProductId = productId,
                OwnerId = ownerId,
            };
        }

        public static void MapExistingLicense(this License license, LicenseEditViewModel model, string productId, string? fileName)
        {
            license.LicenseCategory = model.LicenseCategory;
            license.TradeName = model.TradeName!;
            license.ActiveIngredients = model.ActiveIngredients!;
            license.DosageForm = model.DosageForm!;
            license.RegistrationNumber = model.RegistrationNumber!;
            license.IssueDate = model.IssueDate;
            license.ExpiryDate = model.ExpiryDate;
            license.CurrentOwnerCompany = model.CurrentOwnerCompany!;
            license.AskingPrice = model.AskingPrice;
            license.LicenseDocument = fileName is not null ? $"/uploads/{fileName}" : license.LicenseDocument;
            license.ProductId = productId;
            license.LicenseStatus = LicenseStatus.Pending;
        }

        public static LicenseDetailsViewModel ToLicenseDetailsViewModel(this License license)
        {
            return new LicenseDetailsViewModel
            {
                Id = license.Id,
                LicenseCategory = license.LicenseCategory,
                TradeName = license.TradeName!,
                ProductName = license.Product.Name,
                OwnerUserName = license.Owner.UserName!, 
                ActiveIngredients = license.ActiveIngredients!,
                DosageForm = license.DosageForm!,
                RegistrationNumber = license.RegistrationNumber!,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                CurrentOwnerCompany = license.CurrentOwnerCompany!,
                AskingPrice = license.AskingPrice,
                LicenseDocument = license.LicenseDocument,
                LicenseStatus = license.LicenseStatus,
                CreatedAt = license.CreatedAt,
            };
        }

        public static LicenseListViewModel ToLicenseListViewModel(this License license)
        {
            return new LicenseListViewModel
            {
                Id = license.Id,
                LicenseCategory = license.LicenseCategory,
                TradeName = license.TradeName!,
                ProductName = license.Product.Name,
                OwnerUserName = license.Owner.UserName!, 
                AskingPrice = license.AskingPrice,
                RegistrationNumber = license.RegistrationNumber,
                LicenseStatus = license.LicenseStatus?.ToString() ?? "Unknown",
                CreatedAt = license.CreatedAt,
            };
        }

        public static LicenseEditViewModel ToLicenseEditViewModel(this License license, List<string> productNames)
        {
            return new LicenseEditViewModel
            {
                Id = license.Id,
                LicenseCategory = license.LicenseCategory,
                TradeName = license.TradeName!,
                ProductName = license.Product.Name,
                ActiveIngredients = license.ActiveIngredients!,
                DosageForm = license.DosageForm!,
                RegistrationNumber = license.RegistrationNumber!,
                IssueDate = license.IssueDate,
                ExpiryDate = license.ExpiryDate,
                CurrentOwnerCompany = license.CurrentOwnerCompany!,
                AskingPrice = license.AskingPrice,
                CurrentLicenseDocument = license.LicenseDocument,
                NewLicenseDocument = null,
                LicenseStatus = license.LicenseStatus,
                ProductNames = productNames,
            };
        }

        public static LicenseOwnerViewModel ToLicenseOwnerViewModel(this UserProfile profile)
        {
            return new LicenseOwnerViewModel
            {
                UserName = profile.User.UserName,
                FullName = profile.FirstName + ' ' + profile.LastName,
                BirthDate = profile.BirthDate,
                Nationality = profile.Nationality!,
                Address = profile.Address,
                Email = profile.User.Email!,
                PhoneNumber = profile.PhoneNumber,
                MemberSince = profile.CreatedAt,
            };
        }

        public static LicenseTransferRequest ToTransferRequest(this License license, string buyerId)
        {
            return new LicenseTransferRequest
            {
                LicenseId = license.Id,
                SellerUserId = license.OwnerId,
                BuyerUserId = buyerId,
                Status = DAL.Constants.RequestStatus.Pending
            };   
        }
    }
}

