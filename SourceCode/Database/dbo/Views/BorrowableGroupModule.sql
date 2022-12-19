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
	LM.RegisteredTime
FROM 
	Module AS M INNER JOIN
	ModuleOwnership AS MO ON MO.ModuleId = M.Id INNER JOIN
	GroupMember AS GM ON GM.PersonId = MO.PersonId INNER JOIN
	Person AS P ON P.Id = GM.PersonId LEFT JOIN
	LayoutParticipant AS LP ON LP.PersonId = P.Id LEFT JOIN
	LayoutModule LM ON LM.LayoutParticipantId = LP.Id AND LM.ModuleId = M.Id LEFT JOIN
	MeetingParticipant AS MP ON MP.Id = LP.MeetingParticipantId
WHERE
	M.FunctionalState > 3 AND -- See public enum ModuleFunctionalState
	GM.MemberMayBorrowMyModules <> 0