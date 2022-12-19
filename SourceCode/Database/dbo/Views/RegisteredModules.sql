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
	GM.Id AS GroupMemberId,
	GM.GroupId AS GroupId,
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
	Module AS M INNER JOIN
	ModuleOwnership AS MO ON MO.ModuleId = M.Id INNER JOIN
	GroupMember AS GM ON GM.PersonId = MO.PersonId INNER JOIN
	LayoutModule LM ON LM.ModuleId = M.Id INNER JOIN
	LayoutParticipant AS LP ON LP.Id = LM.LayoutParticipantId INNER JOIN
	MeetingParticipant AS MP ON MP.Id = LP.MeetingParticipantId INNER JOIN
	Person AS P ON P.Id = MP.PersonId
WHERE
	MP.CancellationTime IS NULL
