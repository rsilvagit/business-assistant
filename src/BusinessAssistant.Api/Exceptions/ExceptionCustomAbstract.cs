using FluentValidation.Results;

namespace BusinessAssistant.Api.Exceptions;

public abstract class ExceptionCustomAbstract<TException> : Exception
    where TException : ExceptionCustomAbstract<TException>, new()
{
    public static Dictionary<string, List<string>> Errors { get; protected set; } = [];

    private string _message = string.Empty;

    public override string Message => _message;

    protected void SetMessage(string message) => _message = message;

    protected ExceptionCustomAbstract() { }

    protected ExceptionCustomAbstract(string message) : base(message)
    {
        _message = message;
    }

    public static TException ValidationResult(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToList());

        var instance = new TException();
        instance.SetMessage("There are validation errors in the provided fields.");
        return instance;
    }
}
