namespace BusinessAssistant.Api.DTOs;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    string Document,
    bool IsActive,
    DateTime CreatedAt);
