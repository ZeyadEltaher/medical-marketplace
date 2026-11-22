using System.ComponentModel.DataAnnotations;

namespace Medical.BLL.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [GmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; } = string.Empty;

        // [Required]
        // [DataType(DataType.Password)]
        // public string Password { get; set; } = string.Empty;

        // [Required]
        // [DataType(DataType.Password)]
        // [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
        // public string ConfirmPassword { get; set; } = string.Empty;
    }
}
