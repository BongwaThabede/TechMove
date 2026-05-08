using TechMove.Services;
using Xunit;

namespace TechMove.Tests;

public class CurrencyServiceTests
{
    [Fact]
    public void ConvertUSDToZAR_MultipliesAndRoundsCorrectly()
    {
        var service = new CurrencyService(new HttpClient(), new Microsoft.Extensions.Logging.Abstractions.NullLogger<CurrencyService>());

        var result = service.ConvertUSDToZAR(10.555m, 18.5m);

        Assert.Equal(195.27m, result);
    }
}
