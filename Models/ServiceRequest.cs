using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Contract is required")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Display(Name = "Cost in ZAR")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostInZAR { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(100)]
        [Display(Name = "Request Number")]
        public string RequestNumber { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [DataType(DataType.Date)]
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Admin Notes")]
        public string? AdminNotes { get; set; }

        public virtual Contract? Contract { get; set; }

        public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }
}