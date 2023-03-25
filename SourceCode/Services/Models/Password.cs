namespace ModulesRegistry.Services.Models;

public class Password
{
    [PasswordPolicy(ErrorMessageResourceName = "PasswordInvalid", ErrorMessageResourceType = typeof(Resources.Strings))]
    public string? Value { get; set; }
    [PasswordPolicy(ErrorMessageResourceName = "PasswordInvalid", ErrorMessageResourceType = typeof(Resources.Strings))]
    public string? ConfirmValue { get; set; }

    public bool IsConfirmed => Value?.Equals(ConfirmValue, StringComparison.Ordinal) == true;
}
