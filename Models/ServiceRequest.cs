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
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cost { get; set; }

        [Display(Name = "Cost in ZAR")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostInZAR { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        // Navigation Property
        public virtual Contract? Contract { get; set; }
    }
}