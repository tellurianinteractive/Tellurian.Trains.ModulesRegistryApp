using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Services
{
    public class Password
    {
        [PasswordPolicy( ErrorMessageResourceName = "PasswordInvalid", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string? Value { get; set; }
        [PasswordPolicy(ErrorMessageResourceName = "PasswordInvalid", ErrorMessageResourceType = typeof(Resources.Strings))]
        public string? ConfirmValue { get; set; }

        public bool IsConfirmed => Value is not null && Value.Equals(ConfirmValue, System.StringComparison.Ordinal);
    }

    public class PasswordPolicy : ValidationAttribute
    {
        public static int MinimumLength { get; set; } = 10;
        public static (int Minimum, string Characters) Letters { get; set; } = (4, "ABCDEFGHIJKLMNOPQRSTUÜVXYZÅÄÆÖØabcdefghijklmnopqrstuüvxyzåäæöø");
        public static (int Minimum, string Characters) Digits { get; set; } = (1, "0123456789");
        public static (int Minimum, string Characters) Special { get; set; } = (1, "!#¤%&?§()[]+*_");

        public override bool IsValid(object? value)
        {
            if (value is null) return false;
            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return false;
                if (text.Length < MinimumLength) return false;
                var letters = 0;
                var digits = 0;
                var special = 0;
                var count = 0;
                foreach (var c in text)
                {
                    if (Letters.Characters.Contains(c)) { count++; letters++; };
                    if (Digits.Characters.Contains(c)) { count++; digits++; };
                    if (Special.Characters.Contains(c)) { count++; special++;  };
                }
                return text.Length == count && letters >= Letters.Minimum && digits >= Digits.Minimum && special >= Special.Minimum;
            }
            return base.IsValid(value);
        }

        public override string ToString() =>
            string.Format(Resources.Strings.PasswordPolicy, MinimumLength,
                Letters.Minimum, Letters.Characters,
                Digits.Minimum, Digits.Characters,
                Special.Minimum, Special.Characters);


    }
}
