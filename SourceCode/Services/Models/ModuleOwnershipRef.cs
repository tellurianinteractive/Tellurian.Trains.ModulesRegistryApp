namespace ModulesRegistry.Services.Models;


public static class ModuleOwnershipRefExtensions
{
    public static string Href(this ModuleOwnershipRef ownershipRef, string objectName, int objectId =0, string ActionName = "") =>
        objectId == 0 && ActionName == "" ?
            ownershipRef.IsPerson ? $"/Persons/{ownershipRef.PersonId}/{objectName}" :
            ownershipRef.IsGroup ? $"/Groups/{ownershipRef.GroupId}/{objectName}" : "" :
        ownershipRef.IsPersonInGroup ?
        $"/{objectName}/{objectId}/{ActionName}/PersonOwned/{ownershipRef.PersonId}" :
        ownershipRef.IsGroup ?
        $"/{objectName}/{objectId}/{ActionName}/GroupOwned/{ownershipRef.GroupId}" :
        $"/{objectName}/{objectId}/{ActionName}/PersonOwned/{ownershipRef.PersonId}";

}
