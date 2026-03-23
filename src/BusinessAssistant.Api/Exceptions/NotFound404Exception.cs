namespace BusinessAssistant.Api.Exceptions;

public class NotFound404Exception : ExceptionCustomAbstract<NotFound404Exception>
{
    public NotFound404Exception() { }
    public NotFound404Exception(string message) : base(message) { }
}
