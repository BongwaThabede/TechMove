using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        [Required]
        [StringLength(100)]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Contract Value (USD)")]
        public decimal ContractValueUSD { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Contract Value (ZAR)")]
        public decimal ContractValueZAR { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        [Display(Name = "Last Modified")]
        public DateTime? LastModifiedDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Agreement Upload Date")]
        public DateTime? AgreementUploadDate { get; set; }

        [StringLength(500)]
        public string? SignedAgreementPath { get; set; }

        [StringLength(255)]
        [Display(Name = "Agreement File Name")]
        public string? SignedAgreementFileName { get; set; }

        [NotMapped]
        [Display(Name = "Days Until Expiry")]
        public int DaysUntilExpiry => (EndDate.Date - DateTime.UtcNow.Date).Days;

        public virtual Client? Client { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public bool IsExpiredOrOnHold() =>
            Status.Equals("Expired", StringComparison.OrdinalIgnoreCase) ||
            Status.Equals("OnHold", StringComparison.OrdinalIgnoreCase);

        public bool IsValidForServiceRequest(DateTime utcToday) =>
            Status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
            StartDate.Date <= utcToday.Date &&
            EndDate.Date >= utcToday.Date;
    }
}