namespace Medical.BLL.ViewModels.AdminDashboard
{
    public class UserDetailsViewModel
    {
        // Account
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // Personal
        public string FullName { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }

        // Verification
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public bool IsProfileCompleted { get; set; }

        // Account Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Activity Summary
        public int TotalLicenses { get; set; }
        public int PendingLicenses { get; set; }
        public int ApprovedLicenses { get; set; }
        public int RejectedLicenses { get; set; }
    }
}