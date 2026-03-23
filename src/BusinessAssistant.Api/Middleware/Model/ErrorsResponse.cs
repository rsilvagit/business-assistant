using System.Net;

namespace BusinessAssistant.Api.Middleware.Model;

public record ErrorsResponse
{
    public string Messages { get; set; } = string.Empty;
    public List<ValidationProperty> ValidationProperties { get; set; } = [];
    internal HttpStatusCode StatusCode { get; set; }

    public override string ToString()
    {
        var validations = ValidationProperties
            .Select(v => $"{v.Property}: {string.Join(", ", v.Messages)}");
        var validationText = string.Join("; ", validations);
        return string.IsNullOrEmpty(validationText) ? Messages : $"{Messages} | {validationText}";
    }
}

public record ValidationProperty(
    string Property,
    List<string> Messages
);
