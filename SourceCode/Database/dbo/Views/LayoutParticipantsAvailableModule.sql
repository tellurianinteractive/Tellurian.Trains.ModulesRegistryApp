CREATE VIEW [dbo].[LayoutParticipantsAvailableModule] AS 
SELECT DISTINCT
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
	MO.OwnedShare,
	NULL AS GroupMemberId,
	NULL AS GroupId,
	P.Id AS PersonId,
	P.FirstName,
	P.MiddleName,
	P.LastName,
	P.FremoOwnerSignature,
	P.CityName,
	P.CountryId,
	P.UserId,
	NULL AS LayoutId,
	NULL AS LayoutParticipantId,
	NULL AS RegisteredTime
FROM 
	Module AS M INNER JOIN
	ModuleOwnership AS MO ON MO.ModuleId = M.Id INNER JOIN
	Person AS P ON P.Id = MO.PersonId 
WHERE
	M.FunctionalState > 3 -- See public enum ModuleFunctionalState