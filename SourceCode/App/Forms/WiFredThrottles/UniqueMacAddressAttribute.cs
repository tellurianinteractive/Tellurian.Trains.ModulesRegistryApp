using ModulesRegistry.Services.Implementations;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.WiFredThrottles;

/// <summary>
/// The MAC address doesn't belong to another throttle (<see cref="IWiFredMacAddresses"/>), checked in the database when the form is
/// posted (Migration step 3.6); the browser can't check it, since client validators are synchronous. The throttle being edited is
/// <see cref="WiFredThrottleFormData.ThrottleId"/>. An empty or malformed address is left to [Required] and [MacAddress].
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UniqueMacAddressAttribute : AsyncValidationAttribute
{
    protected override async Task<ValidationResult?> IsValidAsync(object? value, ValidationContext validationContext, CancellationToken cancellationToken)
    {
        if (value is not string macAddress || !macAddress.AsMacAddress().IsMacAddress) return ValidationResult.Success;
        var throttleId = (validationContext.ObjectInstance as WiFredThrottleFormData)?.ThrottleId ?? 0;
        var macAddresses = validationContext.GetRequiredService<IWiFredMacAddresses>();
        return await macAddresses.IsMacAddressTakenAsync(macAddress, throttleId, cancellationToken) ?
            new ValidationResult(FormatErrorMessage(validationContext.DisplayName), [validationContext.MemberName!]) :
            ValidationResult.Success;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        throw new InvalidOperationException("The MAC address is checked in the database, asynchronously only.");
}