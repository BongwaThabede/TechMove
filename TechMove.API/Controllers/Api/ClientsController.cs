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
[Authorize] // Require authentication for all endpoints
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/clients
    [HttpGet]
    [Authorize(Roles = "Admin,FinanceOfficer,ContractManager")]
    public async Task<ActionResult<PagedResponse<ClientResponse>>> GetClients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? region = null)
    {
        var query = _context.Clients.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) || 
                                    c.ContactDetails.Contains(searchTerm));
        }
        if (!string.IsNullOrEmpty(region))
        {
            query = query.Where(c => c.Region == region);
        }

        var totalCount = await query.CountAsync();
        
        var clients = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                ContactDetails = c.ContactDetails,
                Region = c.Region,
                CreatedDate = c.CreatedDate,
                ActiveContractsCount = c.Contracts.Count(cc => cc.Status == "Active")
            })
            .ToListAsync();

        return Ok(new PagedResponse<ClientResponse>(clients, totalCount, pageNumber, pageSize));
    }

    // GET: api/v1/clients/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,FinanceOfficer,ContractManager,Client")]
    public async Task<ActionResult<ClientResponse>> GetClient(int id)
    {
        var client = await _context.Clients
            .Where(c => c.Id == id)
            .Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                ContactDetails = c.ContactDetails,
                Region = c.Region,
                CreatedDate = c.CreatedDate,
                ActiveContractsCount = c.Contracts.Count(cc => cc.Status == "Active")
            })
            .FirstOrDefaultAsync();

        if (client == null) return NotFound();

        // Authorization: Clients can only view their own data
        if (User.IsInRole("Client"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userClientId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { ClientId = EF.Property<int?>(u, "ClientId") })
                .FirstOrDefaultAsync();
            
            if (userClientId?.ClientId != id)
            {
                return Forbid();
            }
        }

        return Ok(client);
    }

    // POST: api/v1/clients
    [HttpPost]
    [Authorize(Roles = "Admin,ContractManager")]
    public async Task<ActionResult<ClientResponse>> CreateClient(CreateClientRequest request)
    {
        var client = new Client
        {
            Name = request.Name,
            ContactDetails = request.ContactDetails,
            Region = request.Region,
            CreatedDate = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var response = new ClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            ContactDetails = client.ContactDetails,
            Region = client.Region,
            CreatedDate = client.CreatedDate,
            ActiveContractsCount = 0
        };

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, response);
    }

    // PUT: api/v1/clients/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContractManager")]
    public async Task<IActionResult> UpdateClient(int id, CreateClientRequest request)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        client.Name = request.Name;
        client.ContactDetails = request.ContactDetails;
        client.Region = request.Region;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/v1/clients/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        // Prevent deletion if client has active contracts
        var hasActiveContracts = await _context.Contracts
            .AnyAsync(c => c.ClientId == id && c.Status == "Active");
        
        if (hasActiveContracts)
        {
            return BadRequest("Cannot delete client with active contracts.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}