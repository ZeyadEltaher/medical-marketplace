using Medical.BLL.ResultModels.AdminDashboardd;
using Medical.BLL.ViewModels.AdminDashboard;

namespace Medical.BLL.Services.Interfaces
{
    public interface IAdminDashboradServices
    {
        Task<List<UserListViewModel>> ListUsersAsync();
        Task<UserGetDetailsModelResult> GetUserDetailsModelAsync(string userId);
    }
}