CREATE VIEW [dbo].[BorrowableGroupModule] AS 
SELECT 
	M.Id AS ModuleId,
	M.FullName AS ModuleName,
	M.ScaleId,
	M.StandardId,
	M.FunctionalState,
	M.Is2R,
	M.Is3R,
	M.LandscapeSeason,
	M.PackageLabel,
	M.ConfigurationLabel,
	M.FremoNumber,
	S.Id AS StationId,
	MO.OwnedShare,
	MO.PersonId AS OwnerPersonId,
	MOP.FirstName AS OwnerFirstName,
	MOP.LastName AS OwnerLastName,
	NULL AS OwnerGroupId,
	NULL AS OwnerGroupName,
	BP.Id AS PersonId,
	BP.FirstName,
	BP.MiddleName,
	BP.LastName,
	BP.FremoOwnerSignature,
	BP.CityName,
	BP.CountryId,
	BP.UserId
FROM 
	[Module] AS M INNER JOIN
	[ModuleOwnership] AS MO ON MO.ModuleId = M.Id INNER JOIN
	[Person] AS MOP ON MOP.Id = MO.PersonId INNER JOIN
	[GroupMember] AS MOGM ON MOGM.PersonId = MOP.Id INNER JOIN
	[GroupMember] AS BGM ON BGM.GroupId = MOGM.GroupId INNER JOIN
	[Person] AS BP ON BGM.PersonId = BP.Id LEFT JOIN
	[Station] AS S ON S.PrimaryModuleId = M.Id
WHERE
	M.FunctionalState >= 3 AND -- See public enum ModuleFunctionalState
	MO.OwnedShare > 0 AND
	MOGM.MemberMayBorrowMyModules <> 0
UNION
SELECT
	M.Id AS ModuleId,
	M.FullName AS ModuleName,
	M.ScaleId,
	M.StandardId,
	M.FunctionalState,
	M.Is2R,
	M.Is3R,
	M.LandscapeSeason,
	M.PackageLabel,
	M.ConfigurationLabel,
	M.FremoNumber,
	S.Id AS StationId,
	MO.OwnedShare,
	MO.PersonId AS OwnerPersonId,
	NULL AS OwnerFirstName,
	NULL AS OwnerLastName,
	G.Id AS OwnerGroupId,
	G.FullName AS OwnerGroupName,
	BP.Id AS PersonId,
	BP.FirstName,
	BP.MiddleName,
	BP.LastName,
	BP.FremoOwnerSignature,
	BP.CityName,
	BP.CountryId,
	BP.UserId
FROM 
	[Module] AS M INNER JOIN
	[ModuleOwnership] AS MO ON MO.ModuleId = M.Id INNER JOIN
	[Group] AS G ON MO.GroupId = G.Id INNER JOIN
	[GroupMember] AS BGM ON BGM.GroupId = G.Id INNER JOIN
	[Person] AS BP ON BGM.PersonId = BP.Id LEFT JOIN
	[Station] AS S ON S.PrimaryModuleId = M.Id
WHERE
	M.FunctionalState >= 3 AND -- See public enum ModuleFunctionalState
	MO.OwnedShare > 0 AND
	BGM.MayBorrowGroupsModules <> 0
