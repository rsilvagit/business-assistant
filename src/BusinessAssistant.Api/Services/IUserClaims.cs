namespace BusinessAssistant.Api.Services;

public interface IUserClaims
{
    Guid AccountId { get; set; }
    string Name { get; set; }
    string Email { get; set; }
    string Role { get; set; }
}

public class UserClaims : IUserClaims
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
