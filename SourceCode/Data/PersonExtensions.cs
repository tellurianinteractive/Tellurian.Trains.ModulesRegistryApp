namespace ModulesRegistry.Data
{
    public static class PersonExtensions
    {
        public static string FullName(this Person me) => me.MiddleName is null ? $"{me.FirstName} {me.LastName}" : $"{me.FirstName} {me.MiddleName} {me.LastName}";
    }
}
