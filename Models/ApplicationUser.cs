using Microsoft.AspNetCore.Identity;
using TechMove.Models;

namespace TechMove.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? ClientId { get; set; }
        
        public string? FullName { get; set; }
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLogin { get; set; }
        
        // Navigation property
        public virtual Client? LinkedClient { get; set; }
    }
}