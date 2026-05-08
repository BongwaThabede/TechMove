namespace TechMove.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUSDToZARRateAsync();
        decimal ConvertUSDToZAR(decimal usdAmount, decimal rate);
    }
}