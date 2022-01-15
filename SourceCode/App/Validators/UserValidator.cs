using FluentValidation;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.EmailAddress)
            .MinimumLength(5)
            .MaximumLength(50)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
    }
}
