using System.ComponentModel.DataAnnotations;
using Medical.BLL.Attributes;

namespace Medical.BLL.ViewModels.UserProfile
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 50 characters.")]
        [ValidUsername(ErrorMessage = "Only lowercase letters, numbers, underscores, and single dots are allowed.")]
        public string UserName { get; set; } = string.Empty;


        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [LettersOnly(ErrorMessage = "First name must contain letters only (no digits).")]
        public string FirstName { get; set; } = string.Empty;
        

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        [LettersOnly(ErrorMessage = "Last name must contain letters only (no digits).")]
        public string LastName { get; set; } = string.Empty;
        

        [Required(ErrorMessage = "Birth date is required.")]
        [ValidBirthDate(minAge: 18, maxAge: 100, ErrorMessage = "You must be between 18 and 100 years old to register.")]
        public DateOnly BirthDate { get; set; }


        [Required(ErrorMessage = "Nationality is required.")]
        [StringLength(50, MinimumLength = 3)]
        [LettersOnly(ErrorMessage = "Nationality must contain letters only.")]
        public string Nationality { get; set; } = string.Empty;
        
        
        [DataType(DataType.PhoneNumber)]
        [StrongPhone(ErrorMessage = "Please enter a valid international phone number.")]
        public string? PhoneNumber { get; set; }
        

        [SafeAddress(ErrorMessage = "Address contains invalid characters.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }
    }
}