using TechMove.Models;
using Xunit;

namespace TechMove.Tests;

public class ContractWorkflowTests
{
    [Fact]
    public void IsValidForServiceRequest_ReturnsTrue_ForActiveContractWithinDates()
    {
        var today = new DateTime(2026, 5, 8);
        var contract = new Contract
        {
            Status = "Active",
            StartDate = today.AddDays(-2),
            EndDate = today.AddDays(10)
        };

        var isValid = contract.IsValidForServiceRequest(today);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Expired")]
    [InlineData("OnHold")]
    public void IsValidForServiceRequest_ReturnsFalse_ForNonActiveStatuses(string status)
    {
        var today = new DateTime(2026, 5, 8);
        var contract = new Contract
        {
            Status = status,
            StartDate = today.AddDays(-2),
            EndDate = today.AddDays(10)
        };

        var isValid = contract.IsValidForServiceRequest(today);

        Assert.False(isValid);
    }
}
