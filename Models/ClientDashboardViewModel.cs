using TechMove.Models;

namespace TechMove.Models
{
    public class ClientDashboardViewModel
    {
        public Client? Client { get; set; }
        public List<Contract> ActiveContracts { get; set; } = new();
        public List<ServiceRequest> OpenRequests { get; set; } = new();
    }
}