using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required")]
        [StringLength(200)]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}