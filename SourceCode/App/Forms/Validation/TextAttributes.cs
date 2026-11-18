using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Validation;

// Custom validation attributes for the form models (Migration section 3.3). Their messages are the resource keys
// "<AttributeName>_Error", found by convention by Microsoft.Extensions.Validation (AddValidation in Program.cs).
// They run on the server only; their JavaScript validators come in step 3.5. Names are not validated for capitals but
// corrected when saved (NameCapitalizationExtensions in Data).

/// <summary>Only Latin letters, digits and some punctuation and symbols (<see cref="TextRules"/>). An empty value is valid.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OrdinaryTextAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is not string text || text.IsOrdinaryText();
}