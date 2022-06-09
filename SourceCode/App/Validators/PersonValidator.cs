using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(person => person.FirstName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .MustBeCapitalizedCorrectly(localizer)
            .WithName(n => localizer[nameof(n.FirstName)]);

        RuleFor(person => person.MiddleName)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .MustBeCapitalizedCorrectly(localizer)
            .WithName(n => localizer[nameof(n.MiddleName)]);

        RuleFor(person => person.LastName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .MustBeCapitalizedCorrectly(localizer)
            .WithName(n => localizer[nameof(n.LastName)]);

        RuleFor(person => person.CityName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .MustBeCapitalizedCorrectly(localizer)
            .WithName(n => localizer[nameof(n.CityName)]);

        RuleFor(person => person.EmailAddresses)
            .MaximumLength(50)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible).When(n => n.EmailAddresses?.Length > 0)
            .WithName(n => localizer[nameof(n.EmailAddresses)]);

        RuleFor(person => person.CountryId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Country)]);

        RuleFor(person => person.FremoOwnerSignature)
            .MaximumLength(10)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.FremoOwnerSignature)]);

        RuleFor(person => person.FremoReservedAdresses)
            .MaximumLength(200)
            .MustBeLocoAdresses(localizer)
            .WithName(n => localizer[nameof(n.FremoReservedAdresses)]);
    }
}
