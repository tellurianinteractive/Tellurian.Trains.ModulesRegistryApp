CREATE VIEW [dbo].[MeetingAdministrator] AS 
SELECT 
	GM.PersonId,
	M.Id AS MeetingId,
	GM.GroupId
FROM 
	[Meeting] M INNER JOIN 
	[GroupMember] GM ON GM.GroupId = M.OrganiserGroupId
WHERE
	GM.IsMeetingAdministrator <> 0
UNION
SELECT 
	GM.PersonId,
	M.Id AS MeetingId,
	GM.GroupId
FROM 
	[Meeting] M INNER JOIN 
	[Layout] L ON L.MeetingId = M.Id INNER JOIN
	[GroupMember] GM ON GM.GroupId = L.OrganisingGroupId
WHERE
	GM.IsMeetingAdministrator <> 0 
UNION
SELECT 
	GM.PersonId, 
	0 AS MeetingId, 
	GM.GroupId 
FROM 
	[GroupMember] AS GM 
WHERE 
	GM.IsMeetingAdministrator <> 0
