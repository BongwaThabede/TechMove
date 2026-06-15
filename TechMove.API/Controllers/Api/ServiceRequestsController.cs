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
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ServiceRequestsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ServiceRequestResponse>>> GetServiceRequests(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null, [FromQuery] string? priority = null,
        [FromQuery] int? contractId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var query = _context.ServiceRequests.Include(sr => sr.Contract).AsQueryable();

        if (!string.IsNullOrEmpty(status)) query = query.Where(sr => sr.Status == status);
        if (!string.IsNullOrEmpty(priority)) query = query.Where(sr => sr.Priority == priority);
        if (contractId.HasValue) query = query.Where(sr => sr.ContractId == contractId.Value);
        if (fromDate.HasValue) query = query.Where(sr => sr.RequestDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(sr => sr.RequestDate <= toDate.Value);

        if (User.IsInRole("Client"))
        {
            var clientId = await GetUserClientIdAsync();
            if (clientId.HasValue) query = query.Where(sr => sr.Contract != null && sr.Contract.ClientId == clientId.Value);
            else return Forbid();
        }

        var totalCount = await query.CountAsync();
        
        // ✅ FIX: Load entities first, then project to DTO in memory (avoids CS8072)
        var serviceRequests = await query
            .OrderByDescending(sr => sr.RequestDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var requests = serviceRequests.Select(sr => new ServiceRequestResponse
        {
            Id = sr.Id,
            RequestNumber = sr.RequestNumber,
            ContractId = sr.ContractId,
            // ✅ FIX: Use explicit null check instead of ?. operator
            ContractClientName = sr.Contract != null && sr.Contract.Client != null ? sr.Contract.Client.Name : "Unknown",
            Description = sr.Description,
            Cost = sr.Cost,
            CostInZAR = sr.CostInZAR,
            Status = sr.Status,
            Priority = sr.Priority,
            RequestDate = sr.RequestDate,
            CreatedDate = sr.CreatedDate,
            CompletedDate = sr.CompletedDate,
            AdminNotes = sr.AdminNotes
        }).ToList();

        return Ok(new PagedResponse<ServiceRequestResponse>(requests, totalCount, pageNumber, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequestResponse>> GetServiceRequest(int id)
    {
        // ✅ FIX: Load entity first, then project to DTO
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null) return NotFound();

        var request = new ServiceRequestResponse
        {
            Id = serviceRequest.Id,
            RequestNumber = serviceRequest.RequestNumber,
            ContractId = serviceRequest.ContractId,
            // ✅ FIX: Explicit null check
            ContractClientName = serviceRequest.Contract != null && serviceRequest.Contract.Client != null 
                ? serviceRequest.Contract.Client.Name 
                : "Unknown",
            Description = serviceRequest.Description,
            Cost = serviceRequest.Cost,
            CostInZAR = serviceRequest.CostInZAR,
            Status = serviceRequest.Status,
            Priority = serviceRequest.Priority,
            RequestDate = serviceRequest.RequestDate,
            CreatedDate = serviceRequest.CreatedDate,
            CompletedDate = serviceRequest.CompletedDate,
            AdminNotes = serviceRequest.AdminNotes
        };

        if (User.IsInRole("Client"))
        {
            var clientId = await GetUserClientIdAsync();
            var contract = await _context.Contracts.FindAsync(request.ContractId);
            if (contract == null || contract.ClientId != clientId) return Forbid();
        }

        return Ok(request);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestResponse>> CreateServiceRequest(CreateServiceRequestRequest requestDto)
    {
        var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == requestDto.ContractId && c.Status == "Active");
        if (contract == null) return BadRequest("Invalid or inactive contract.");

        if (User.IsInRole("Client"))
        {
            var clientId = await GetUserClientIdAsync();
            if (contract.ClientId != clientId) return Forbid();
        }

        var serviceRequest = new ServiceRequest
        {
            ContractId = requestDto.ContractId,
            Description = requestDto.Description,
            Priority = requestDto.Priority,
            Status = "Pending",
            Cost = 0,
            CostInZAR = 0,
            RequestDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            RequestNumber = $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();
        
        var response = await MapToResponseAsync(serviceRequest.Id);
        return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,LogisticsCoordinator,ContractManager,FinanceOfficer")]
    public async Task<IActionResult> UpdateServiceRequest(int id, UpdateServiceRequestRequest requestDto)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);
        if (serviceRequest == null) return NotFound();

        if (requestDto.Description != null) serviceRequest.Description = requestDto.Description;
        if (requestDto.Status != null) serviceRequest.Status = requestDto.Status;
        if (requestDto.Cost.HasValue) serviceRequest.Cost = requestDto.Cost.Value;
        if (requestDto.CostInZAR.HasValue) serviceRequest.CostInZAR = requestDto.CostInZAR.Value;
        if (requestDto.AdminNotes != null) serviceRequest.AdminNotes = requestDto.AdminNotes;

        if (requestDto.CompletedDate.HasValue)
        {
            serviceRequest.CompletedDate = requestDto.CompletedDate.Value;
            if (serviceRequest.Status != "Completed") serviceRequest.Status = "Completed";
        }
        else if (requestDto.Status == "Completed" && serviceRequest.CompletedDate == null)
        {
            serviceRequest.CompletedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);
        if (serviceRequest == null) return NotFound();
        if (serviceRequest.Status == "Completed") return BadRequest("Cannot delete completed service requests for audit compliance.");

        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ✅ FIX: Load entity first, then project to DTO (avoids CS8072)
    private async Task<ServiceRequestResponse> MapToResponseAsync(int id)
    {
        var sr = await _context.ServiceRequests
            .Include(s => s.Contract)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (sr == null) throw new InvalidOperationException($"ServiceRequest {id} not found");

        return new ServiceRequestResponse
        {
            Id = sr.Id,
            RequestNumber = sr.RequestNumber,
            ContractId = sr.ContractId,
            // ✅ FIX: Explicit null check
            ContractClientName = sr.Contract != null && sr.Contract.Client != null 
                ? sr.Contract.Client.Name 
                : "Unknown",
            Description = sr.Description,
            Cost = sr.Cost,
            CostInZAR = sr.CostInZAR,
            Status = sr.Status,
            Priority = sr.Priority,
            RequestDate = sr.RequestDate,
            CreatedDate = sr.CreatedDate,
            CompletedDate = sr.CompletedDate,
            AdminNotes = sr.AdminNotes
        };
    }

    private async Task<int?> GetUserClientIdAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return null;
        
        var claims = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { ClientId = EF.Property<int?>(u, "ClientId") })
            .FirstOrDefaultAsync();
        
        return claims?.ClientId;
    }
}