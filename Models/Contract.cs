using System;
using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client is required")]
        public int ClientId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, Active, Expired, OnHold

        [Required]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        // File Handling
        public string? SignedAgreementPath { get; set; }
        public string? SignedAgreementFileName { get; set; }

        // Navigation Properties
        public virtual Client? Client { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // Validation
        public bool IsExpiredOrOnHold()
        {
            return Status.Equals("Expired", StringComparison.OrdinalIgnoreCase) ||
                   Status.Equals("OnHold", StringComparison.OrdinalIgnoreCase);
        }
    }
}