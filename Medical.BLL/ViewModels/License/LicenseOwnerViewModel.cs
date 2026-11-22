using System.ComponentModel.DataAnnotations;

namespace Medical.BLL.ViewModels.License
{
    public class LicenseOwnerViewModel
    {
        public string? UserName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
    }
}