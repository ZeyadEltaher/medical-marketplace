using Medical.BLL.ViewModels.UserRequest;
using Medical.DAL.Entities;

namespace Medical.BLL.Mappers;

public static class UserRequestMapper
{
    public static RequestListViewModel ToRequestListViewModel(this LicenseTransferRequest? request)
    {
        if (request is null) return null!;

        return new RequestListViewModel
        {
            Id = request.Id,
            TradeName = request.License.TradeName,
            LicenseCategory = request.License.LicenseCategory,
            SellerUserName = request.Seller.UserName ?? string.Empty,
            BuyerUserName = request.Buyer.UserName ?? string.Empty,
            AskingPrice = request.License.AskingPrice,
            LicenseStatus = request.License.LicenseStatus,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }

    public static RequestDetailsViewModel ToRequestDetailsViewModel(this LicenseTransferRequest? request)
    {
        if (request is null) return null!;

        return new RequestDetailsViewModel
        {
            Id = request.Id,
            TradeName = request.License.TradeName,
            LicenseCategory = request.License.LicenseCategory,
            ActiveIngredients = request.License.ActiveIngredients,
            DosageForm = request.License.DosageForm,
            RegistrationNumber = request.License.RegistrationNumber,
            IssueDate = request.License.IssueDate,
            ExpiryDate = request.License.ExpiryDate,
            CurrentOwnerCompany = request.License.CurrentOwnerCompany,
            AskingPrice = request.License.AskingPrice,
            SellerUserName = request.Seller.UserName ?? string.Empty,
            BuyerUserName = request.Buyer.UserName ?? string.Empty,
            LicenseStatus = request.License.LicenseStatus,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }
}