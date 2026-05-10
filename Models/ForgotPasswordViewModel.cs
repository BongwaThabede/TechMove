using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
