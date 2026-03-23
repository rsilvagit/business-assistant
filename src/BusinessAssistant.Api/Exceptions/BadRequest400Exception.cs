namespace BusinessAssistant.Api.Exceptions;

public class BadRequest400Exception : ExceptionCustomAbstract<BadRequest400Exception>
{
    public BadRequest400Exception() { }
    public BadRequest400Exception(string message) : base(message) { }
}
