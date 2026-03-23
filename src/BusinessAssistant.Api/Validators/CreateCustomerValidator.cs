using BusinessAssistant.Api.DTOs;
using FluentValidation;

namespace BusinessAssistant.Api.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .MaximumLength(20);

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Document is required.")
            .MaximumLength(20);
    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .MaximumLength(20);

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Document is required.")
            .MaximumLength(20);
    }
}
