using Medical.DAL.Identity;

namespace Medical.DAL.Entities
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}