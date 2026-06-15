using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.API.Controllers.Api.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IContractStatusService _statusService;
    private readonly ICurrencyService _currencyService;

    public ContractsController(ApplicationDbContext context, IContractStatusService statusService, ICurrencyService currencyService)
    {
        _context = context;
        _statusService = statusService;
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Dtos.Responses.ContractResponse>>> GetContracts([FromQuery] string? status)
    {
        await _statusService.SyncAllAsync(DateTime.UtcNow.Date);

        var query = _context.Contracts.Include(c => c.Client).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "All")
            query = query.Where(c => c.Status == status);

        var contracts = await query
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => new Dtos.Responses.ContractResponse
            {
                Id = c.Id,
                ClientId = c.ClientId,
                ClientName = c.Client != null ? c.Client.Name : "Unknown",
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                ServiceLevel = c.ServiceLevel,
                ContractNumber = c.ContractNumber,
                ContractValueUSD = c.ContractValueUSD,
                ContractValueZAR = c.ContractValueZAR,
                CreatedDate = c.CreatedDate,
                LastModifiedDate = c.LastModifiedDate,
                DaysUntilExpiry = (c.EndDate - DateTime.UtcNow).Days
            })
            .ToListAsync();

        return Ok(contracts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dtos.Responses.ContractResponse>> GetContract(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) return NotFound();

        if (_statusService.SyncSingle(contract, DateTime.UtcNow.Date))
            await _context.SaveChangesAsync();

        return Ok(new Dtos.Responses.ContractResponse
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
    public async Task<ActionResult<Dtos.Responses.ContractResponse>> CreateContract(Dtos.Requests.CreateContractRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var clientExists = await _context.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists) return BadRequest("Client does not exist.");

        var rate = await _currencyService.GetUSDToZARRateAsync();
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

        var response = new Dtos.Responses.ContractResponse
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
    public async Task<IActionResult> UpdateContract(int id, Dtos.Requests.CreateContractRequest request)
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
        contract.ContractValueZAR = request.ContractValueUSD * await _currencyService.GetUSDToZARRateAsync();
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