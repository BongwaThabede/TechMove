using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;
using TechMove.Models;

namespace TechMove.API.Controllers.Api.v1;

[ApiController]
[Route("api/v1/[controller]")]
// [Authorize]
public class ContractsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContractsController(ApplicationDbContext context)
    {
        _context = context;
    }

   [HttpGet]
public async Task<ActionResult<IEnumerable<ContractResponse>>> GetContracts([FromQuery] string? status)
{
    // ✅ TEMPORARY: return hardcoded data for demo
    var contracts = new List<ContractResponse>
    {
        new ContractResponse
        {
            Id = 1,
            ClientId = 1,
            ClientName = "Acme Global",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddMonths(6),
            Status = "Active",
            ServiceLevel = "Gold",
            ContractNumber = "CT-001",
            ContractValueUSD = 10000,
            ContractValueZAR = 185000,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            DaysUntilExpiry = 180
        }
    };
    return Ok(contracts);
}
    [HttpGet("{id}")]
    public async Task<ActionResult<ContractResponse>> GetContract(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();

        return Ok(new ContractResponse
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            ClientName = contract.Client?.Name ?? "Unknown",
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel,
            ContractNumber = contract.ContractNumber,
            ContractValueUSD = contract.ContractValueUSD,
            ContractValueZAR = contract.ContractValueZAR,
            CreatedDate = contract.CreatedDate,
            LastModifiedDate = contract.LastModifiedDate,
            DaysUntilExpiry = (contract.EndDate - DateTime.UtcNow).Days
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,LogisticsManager")]
    public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists) return BadRequest("Client does not exist.");

        // Hardcoded exchange rate
        var rate = 18.5m;

        var contract = new Contract
        {
            ClientId = request.ClientId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status ?? "Draft",
            ServiceLevel = request.ServiceLevel,
            ContractNumber = request.ContractNumber,
            ContractValueUSD = request.ContractValueUSD,
            ContractValueZAR = request.ContractValueUSD * rate,
            CreatedDate = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        var response = new ContractResponse
        {
            Id = contract.Id,
            ClientId = contract.ClientId,
            ClientName = (await _context.Clients.FindAsync(contract.ClientId))?.Name ?? "Unknown",
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status,
            ServiceLevel = contract.ServiceLevel,
            ContractNumber = contract.ContractNumber,
            ContractValueUSD = contract.ContractValueUSD,
            ContractValueZAR = contract.ContractValueZAR,
            CreatedDate = contract.CreatedDate,
            DaysUntilExpiry = (contract.EndDate - DateTime.UtcNow).Days
        };
        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, response);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,LogisticsManager")]
    public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] UpdateContractStatusRequest request)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        contract.Status = request.Status;
        contract.LastModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] CreateContractRequest request)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();

        contract.ClientId = request.ClientId;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.Status = request.Status ?? contract.Status;
        contract.ServiceLevel = request.ServiceLevel;
        contract.ContractNumber = request.ContractNumber;
        contract.ContractValueUSD = request.ContractValueUSD;
        contract.ContractValueZAR = request.ContractValueUSD * 18.5m;
        contract.LastModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return NotFound();
        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}