namespace BusinessAssistant.Api.DTOs;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Document);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone,
    string Document);
