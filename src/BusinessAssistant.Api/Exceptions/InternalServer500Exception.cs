namespace BusinessAssistant.Api.Exceptions;

public class InternalServer500Exception : ExceptionCustomAbstract<InternalServer500Exception>
{
    public InternalServer500Exception() { }
    public InternalServer500Exception(string message) : base(message) { }
}
