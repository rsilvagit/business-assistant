namespace BusinessAssistant.Api.Models;

public class PasswordModel
{
    public Guid Id { get; set; }
    public Guid Salt { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool? Actived { get; set; }
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Account { get; set; } = null!;
}
