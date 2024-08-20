CREATE VIEW [dbo].[MeetingAdministrator] AS 
SELECT 
	GM.PersonId,
	M.Id AS MeetingId
FROM 
	[Meeting] M INNER JOIN 
	[GroupMember] GM ON GM.GroupId = M.OrganiserGroupId
WHERE
	GM.IsMeetingAdministrator <> 0
UNION
SELECT 
	GM.PersonId,
	M.Id AS MeetingId
FROM 
	[Meeting] M INNER JOIN 
	[Layout] L ON L.MeetingId = M.Id INNER JOIN
	[GroupMember] GM ON GM.GroupId = L.OrganisingGroupId
WHERE
	GM.IsMeetingAdministrator <> 0 
