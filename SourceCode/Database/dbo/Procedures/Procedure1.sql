CREATE PROCEDURE [dbo].[GetMeetingAdministrators]
	@MeetingId int = 0
AS
	SELECT 
	GM.PersonId
FROM 
	[Meeting] M INNER JOIN 
	[GroupMember] GM ON GM.GroupId = M.OrganiserGroupId
	INNER JOIN [Person] P ON P.Id = GM.PersonId
WHERE
	GM.IsMeetingAdministrator <> 0 AND
	M.Id = @MeetingId
UNION
SELECT 
	GM.PersonId
FROM 
	[Meeting] M INNER JOIN 
	[Layout] L ON L.MeetingId = M.Id INNER JOIN
	[GroupMember] GM ON GM.GroupId = L.OrganisingGroupId
	INNER JOIN [Person] P ON P.Id = GM.PersonId
WHERE
	GM.IsMeetingAdministrator <> 0 AND
	M.Id = @MeetingId

RETURN 0
