using System.ComponentModel.DataAnnotations;

namespace Medical.BLL.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [GmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
