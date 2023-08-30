namespace ModulesRegistry.Services.Models;


public static class ModuleOwnershipRefExtensions
{
    public static string Href(this ModuleOwnershipRef ownershipRef, string objectName, int objectId, string ActionName) =>
        ownershipRef.IsPersonInGroup ?
        $"/{objectName}/{objectId}/{ActionName}/PersonOwned/{ownershipRef.PersonId}/InGroup/{ownershipRef.GroupId}" :
        ownershipRef.IsGroup ?
        $"/{objectName}/{objectId}/{ActionName}/GroupOwned/{ownershipRef.GroupId}" :
        $"/{objectName}/{objectId}/{ActionName}/PersonOwned/{ownershipRef.PersonId}";

}
