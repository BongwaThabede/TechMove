using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        public static readonly string[] CompanyTypeOptions =
        {
            "Shipper",
            "Consignee",
            "Freight Forwarder",
            "Carrier/Shipping Line",
            "3PL Provider",
            "Customs Broker",
            "Warehousing/Distribution",
            "Manufacturer/Retailer",
            "Other"
        };

        private static readonly HashSet<string> AllowedAccountTypes = new(StringComparer.Ordinal)
        {
            "LogisticsManager",
            "GeneralUser",
            "Viewer"
        };

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [StringLength(30, MinimumLength = 7, ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least {2} characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(200)]
        [Display(Name = "Company name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company type is required.")]
        [Display(Name = "Company type")]
        public string CompanyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account type.")]
        [Display(Name = "Account type")]
        public string AccountType { get; set; } = string.Empty;

        [Display(Name = "I agree to Terms & Conditions")]
        public bool AgreeTerms { get; set; }

        [Display(Name = "I agree to Privacy Policy")]
        public bool AgreePrivacy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!AgreeTerms)
            {
                yield return new ValidationResult(
                    "You must agree to the Terms & Conditions.",
                    new[] { nameof(AgreeTerms) });
            }

            if (!AgreePrivacy)
            {
                yield return new ValidationResult(
                    "You must agree to the Privacy Policy.",
                    new[] { nameof(AgreePrivacy) });
            }

            if (!string.IsNullOrWhiteSpace(AccountType) && !AllowedAccountTypes.Contains(AccountType))
            {
                yield return new ValidationResult("Invalid account type.", new[] { nameof(AccountType) });
            }

            if (!string.IsNullOrWhiteSpace(CompanyType) &&
                !CompanyTypeOptions.Contains(CompanyType))
            {
                yield return new ValidationResult("Please select a valid company type.", new[] { nameof(CompanyType) });
            }
        }
    }
}
