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
	GM.PersonId AS BorrowerPersonId
FROM 
	[Module] AS M INNER JOIN
	[ModuleOwnership] AS MO ON MO.ModuleId = M.Id INNER JOIN
	[GroupMember] AS GM ON GM.GroupId = MO.GroupId INNER JOIN
	[Group] G ON G.Id = GM.GroupId INNER JOIN
	[Person] P ON P.Id = GM.PersonId
WHERE
	M.FunctionalState > 3 AND -- See public enum ModuleFunctionalState
	GM.MemberMayBorrowMyModules <> 0 