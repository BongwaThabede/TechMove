using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required")]
        [StringLength(200)]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        // Navigation Property
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}