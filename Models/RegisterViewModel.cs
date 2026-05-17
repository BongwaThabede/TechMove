using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMove.Models
{
    public class RegisterViewModel
    {
        // Personal Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Company Information
        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company type is required")]
        [Display(Name = "Company Type")]
        public string CompanyType { get; set; } = string.Empty;

        // Account Type - Optional
        [Display(Name = "Account Type")]
        public string AccountType { get; set; } = string.Empty;

        // Agreements - Changed from Range to just a boolean property
        [Display(Name = "I agree to Terms & Conditions")]
        public bool AgreeTerms { get; set; }

        [Display(Name = "I agree to Privacy Policy")]
        public bool AgreePrivacy { get; set; }

        // Dropdown Options
        public static readonly List<string> CompanyTypeOptions = new()
        {
            "Logistics & Supply Chain",
            "Freight Forwarding",
            "Warehousing",
            "Manufacturing",
            "Retail",
            "Technology",
            "Other"
        };

        public static readonly List<SelectListItem> AccountTypeOptions = new()
        {
            new SelectListItem { Value = "Admin", Text = "Administrator" },
            new SelectListItem { Value = "LogisticsManager", Text = "Logistics Manager" },
            new SelectListItem { Value = "Finance", Text = "Finance Officer" },
            new SelectListItem { Value = "ContractManager", Text = "Contract Manager" }
        };
    }
}