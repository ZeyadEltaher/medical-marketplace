using System.ComponentModel.DataAnnotations;

namespace Medical.BLL.ViewModels.Account
{
    public class ForgetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [GmailAddress(ErrorMessage = "Please enter a valid email.")]
        public string? Email { get; set; }
    }
}
