using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;

namespace TechMove.Services;

public interface IApiClient
{
    Task<List<ContractResponse>> GetContractsAsync(string? status = null);
    Task<ContractResponse?> GetContractAsync(int id);
    Task<ContractResponse> CreateContractAsync(CreateContractRequest request);
    Task<bool> UpdateContractStatusAsync(int id, string status);
    Task<bool> DeleteContractAsync(int id);
    Task<List<ClientResponse>> GetClientsAsync();
    Task CreateContractAsync(Dtos.Requests.CreateContractRequest contract);
}