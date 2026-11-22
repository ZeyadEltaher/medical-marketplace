using System.ComponentModel.DataAnnotations;

namespace Medical.BLL.ViewModels.UserProfile
{
    public class ProfileDetailsViewModel
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneConfirmed { get; set; }
    }
}