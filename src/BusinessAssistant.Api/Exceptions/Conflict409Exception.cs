namespace BusinessAssistant.Api.Exceptions;

public class Conflict409Exception : ExceptionCustomAbstract<Conflict409Exception>
{
    public Conflict409Exception() { }
    public Conflict409Exception(string message) : base(message) { }
}
