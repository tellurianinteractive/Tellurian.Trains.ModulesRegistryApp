using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data;
public class AvailableModule
{
    public int ModuleId { get; set; }
    public required string FullName { get; set; }
    public int ScaleId { get; set; }
    public int StandardId { get; set; }
    public int FunctionalState { get; set; }
    public bool Is2R { get; set; }
    public bool Is3R { get; set;}
    public int LandscapeSeason { get; set; }
    public string? PackageLabel { get; set; }
    public string? ConfigurationLabel { get; set;}
    public int? FremoNumber { get; set; }
    public int? StationId { get; set; }
    public int OwnedShare { get; set; }
    public int? OwnerPersonId { get; set; }
    public string? OwnerFirstName { get; set;}
    public string? OwnerLastName { get; set;}
    public int? OwnerGroupId { get; set; }
    public string? OwnerGroupName { get; set;} 
    public int PersonId { get; set; }
    public required string FirstName { get; set;}
    public string? MiddleName { get; set; }
    public required string LastName { get; set;}
    public string? FremoOwnerSignature { get; set; }
    public required string CityName { get; set; }
    public int CountryId { get; set; }
    public int? UserId { get; set; }
}

