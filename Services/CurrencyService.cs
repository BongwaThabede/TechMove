using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TechMove.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetUSDToZARRateAsync()
        {
            try
            {
                // Using ExchangeRate-API (Free, no key required)
                var response = await _httpClient.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("rates", out var rates) && 
                        rates.TryGetProperty("ZAR", out var zarRate))
                    {
                        var rate = zarRate.GetDecimal();
                        _logger.LogInformation("Successfully retrieved USD to ZAR rate: {Rate}", rate);
                        return rate;
                    }
                }
                
                // Fallback rate if API fails
                _logger.LogWarning("API failed, using fallback rate");
                return 18.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching currency rate");
                return 18.50m; // Fallback rate
            }
        }

        public decimal ConvertUSDToZAR(decimal usdAmount, decimal rate)
        {
            return Math.Round(usdAmount * rate, 2);
        }
    }
}