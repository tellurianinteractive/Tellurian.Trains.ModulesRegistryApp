CREATE VIEW [dbo].[RegisteredModules] AS 
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
	MO.OwnedShare,
	MO.PersonId AS OwnerPersonId,
	MOP.FirstName AS OwnerFirstName,
	MOP.LastName AS OwnerLastName,
	MO.GroupId AS OwnerGroupId,
	GOP.FullName AS OwnerGroupName,
	P.Id AS PersonId,
	P.FirstName,
	P.MiddleName,
	P.LastName,
	P.FremoOwnerSignature,
	P.CityName,
	P.CountryId,
	P.UserId,
	LP.LayoutId,
	LM.LayoutParticipantId,
	LM.RegisteredTime AS ModuleRegistrationTime,
	LP.MeetingParticipantId,
	MP.RegistrationTime
FROM 
	Person AS P INNER JOIN
	MeetingParticipant AS MP ON MP.PersonId = P.Id INNER JOIN
	LayoutParticipant AS LP ON LP.MeetingParticipantId = MP.Id INNER JOIN
	LayoutModule LM ON LM.LayoutParticipantId = LP.Id INNER JOIN
	Module AS M ON LM.ModuleId = M.Id INNER JOIN
	ModuleOwnership AS MO ON MO.ModuleId = M.Id LEFT JOIN
	Person AS MOP ON MOP.Id = MO.PersonId LEFT JOIN
	[Group] AS GOP ON GOP.Id = MO.GroupId
WHERE
	MP.CancellationTime IS NULL AND
	MO.OwnedShare > 0
