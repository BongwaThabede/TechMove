using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;

namespace TechMove.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private void SetAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
        if (!string.IsNullOrEmpty(token))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<ContractResponse>> GetContractsAsync(string? status = null)
    {
        SetAuthorizationHeader();
        var url = "/api/v1/contracts";
        if (!string.IsNullOrEmpty(status) && status != "All")
            url += $"?status={status}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ContractResponse>>() ?? new();
    }

    public async Task<ContractResponse?> GetContractAsync(int id)
    {
        SetAuthorizationHeader();
        var response = await _httpClient.GetAsync($"/api/v1/contracts/{id}");
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ContractResponse>() : null;
    }

    public async Task<ContractResponse> CreateContractAsync(CreateContractRequest request)
    {
        SetAuthorizationHeader();
        var response = await _httpClient.PostAsJsonAsync("/api/v1/contracts", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ContractResponse>()
            ?? throw new Exception("Failed to create contract");
    }

    public async Task<bool> UpdateContractStatusAsync(int id, string status)
    {
        SetAuthorizationHeader();
        var updateDto = new UpdateContractStatusRequest { Status = status };
        var response = await _httpClient.PatchAsJsonAsync($"/api/v1/contracts/{id}/status", updateDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        SetAuthorizationHeader();
        var response = await _httpClient.DeleteAsync($"/api/v1/contracts/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClientResponse>> GetClientsAsync()
    {
        SetAuthorizationHeader();
        var response = await _httpClient.GetAsync("/api/v1/clients?pageSize=100");
        response.EnsureSuccessStatusCode();
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ClientResponse>>();
        return pagedResponse?.Data?.ToList() ?? new();
    }
}