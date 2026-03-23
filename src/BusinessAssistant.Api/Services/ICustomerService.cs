using BusinessAssistant.Api.DTOs;

namespace BusinessAssistant.Api.Services;

public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
    Task<CustomerResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<CustomerResponse>> GetAllAsync();
    Task<CustomerResponse?> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task<bool> DeleteAsync(Guid id);
}
