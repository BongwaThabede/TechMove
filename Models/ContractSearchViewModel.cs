using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechMove.Models
{
    public class ContractSearchViewModel
    {
        // Search Filters
        [Display(Name = "Client")]
        public int? ClientId { get; set; }
        
        [Display(Name = "Status")]
        public string? Status { get; set; }
        
        [Display(Name = "Service Level")]
        public string? ServiceLevel { get; set; }
        
        [Display(Name = "Start Date From")]
        [DataType(DataType.Date)]
        public DateTime? StartDateFrom { get; set; }
        
        [Display(Name = "Start Date To")]
        [DataType(DataType.Date)]
        public DateTime? StartDateTo { get; set; }
        
        [Display(Name = "End Date From")]
        [DataType(DataType.Date)]
        public DateTime? EndDateFrom { get; set; }
        
        [Display(Name = "End Date To")]
        [DataType(DataType.Date)]
        public DateTime? EndDateTo { get; set; }
        
        [Display(Name = "Min Value (USD)")]
        [DataType(DataType.Currency)]
        public decimal? MinValueUSD { get; set; }
        
        [Display(Name = "Max Value (USD)")]
        [DataType(DataType.Currency)]
        public decimal? MaxValueUSD { get; set; }
        
        // Search Results
        public List<Contract> Results { get; set; } = new();
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        
        // Dropdown Helpers
        public List<SelectListItem>? Clients { get; set; }
        
        public static List<SelectListItem> StatusOptions => new()
        {
            new SelectListItem { Value = "", Text = "-- All Statuses --" },
            new SelectListItem { Value = "Draft", Text = "Draft" },
            new SelectListItem { Value = "Active", Text = "Active" },
            new SelectListItem { Value = "OnHold", Text = "On Hold" },
            new SelectListItem { Value = "Expired", Text = "Expired" }
        };
        
        public static List<SelectListItem> ServiceLevelOptions => new()
        {
            new SelectListItem { Value = "", Text = "-- All Levels --" },
            new SelectListItem { Value = "Basic", Text = "Basic" },
            new SelectListItem { Value = "Standard", Text = "Standard" },
            new SelectListItem { Value = "Premium", Text = "Premium" },
            new SelectListItem { Value = "Enterprise", Text = "Enterprise" }
        };
    }
}