using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(user => user.EmailAddress)
            .MinimumLength(5)
            .MaximumLength(50)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

        RuleFor(user => user.AdministratorAreaOfResposibility)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer);

        RuleFor(user => user.FailedLoginAttempts)
            .GreaterThanOrEqualTo(0);

        RuleFor(user => user.PasswordResetAttempts)
            .GreaterThanOrEqualTo(0);
    }
}
