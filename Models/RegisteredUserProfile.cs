namespace TechMove.Models
{
    /// <summary>
    /// Extra registration fields stored in memory for demo accounts.
    /// </summary>
    public class RegisteredUserProfile
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyType { get; set; } = string.Empty;
    }
}
