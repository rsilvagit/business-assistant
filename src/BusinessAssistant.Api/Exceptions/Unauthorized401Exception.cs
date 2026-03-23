namespace BusinessAssistant.Api.Exceptions;

public class Unauthorized401Exception : ExceptionCustomAbstract<Unauthorized401Exception>
{
    public Unauthorized401Exception() { }
    public Unauthorized401Exception(string message) : base(message) { }
}
