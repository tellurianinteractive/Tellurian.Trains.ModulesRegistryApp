namespace ModulesRegistry.Services.Extensions;

public static class ExceptionExtensions
{
    public static string ErrorMessage(this Exception ex, IEnumerable<ErrorCase> errorCases)
    {
        if (ex.InnerException is null) return ex.Message;
        foreach (var errorCase in errorCases)
        {
            if (ex.InnerException.Message.Contains(errorCase.ConstraintName, StringComparison.OrdinalIgnoreCase)) 
                return $"{"Error".Localized()}: {errorCase.ErrorResouceCode.Localized()} '{errorCase.ObjectResourceCode.Localized()}'";
        }
        return ex.InnerException.Message;
    }
}

public record ErrorCase(string ConstraintName, string ErrorResouceCode, string ObjectResourceCode);
