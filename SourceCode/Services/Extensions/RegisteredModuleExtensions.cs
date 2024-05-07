using System.Data;

namespace ModulesRegistry.Services.Extensions;
internal static class RegisteredModuleExtensions
{
    public static string PackageName(this RegisteredModule module) => module.PackageLabel.HasValue() ? module.PackageLabel : module.Name;

    public static RegisteredModule MapRegisteredModule(this IDataRecord record) =>
        new()
        {
            Id = record.GetInt("ModuleId"),
            Name = record.GetString("ModuleName"),
            PackageLabel = record.GetString("PackageLabel"),
            ConfigurationLabel = record.GetString("ConfigurationLabel"),
            FremoNumber = record.GetNullableInt("FremoNumber", null),
            OwnerName = record.OwnerName(),
            ParticipantPersonId = record.GetInt("PersonId"),
            ParticipantName = record.ParticipantName(),
            ModuleRegistrationTime = record.GetDateTimeOffset("ModuleRegistrationTime"),
            MeetingParticipantId = record.GetInt("MeetingParticipantId"),
            ParticipantRegistrationTime = record.GetDateTimeOffset("RegistrationTime"),
        };

    private static string OwnerName(this IDataRecord record)
    {
        var firstName = record.GetString("OwnerFirstName");
        if (firstName == null)
        {
            return record.GetString("OwnerGroupName");
        }
        var lastName = record.GetString("OwnerLastName");
        return $"{firstName} {lastName}";
    }

    private static string ParticipantName (this IDataRecord record)
    {
        var firstName = record.GetString("FirstName");
        var lastName = record.GetString("LastName");
        var cityName = record.GetString("CityName");
        return $"{firstName} {lastName}, {cityName}";
    }

}

public class RegisteredModule
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? PackageLabel { get; init; }
    public string? ConfigurationLabel { get; init; }
    public int? FremoNumber { get; init; }
    public required string OwnerName { get; init; }
    public int ParticipantPersonId { get; init; }
    public required string ParticipantName { get; init; }
    public DateTimeOffset ModuleRegistrationTime { get; init; }
    public int MeetingParticipantId { get; init; }
    public DateTimeOffset ParticipantRegistrationTime { get; init; }


}
