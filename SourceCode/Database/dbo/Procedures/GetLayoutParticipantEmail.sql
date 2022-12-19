CREATE PROCEDURE [dbo].[GetLayoutParticipantEmail]
	@LayoutId INT
AS
	SELECT
	P.EmailAddresses + ';'
FROM 
	LayoutParticipant AS LP INNER JOIN
	MeetingParticipant AS MP ON MP.Id = LP.MeetingParticipantId INNER JOIN
	Person AS P ON P.Id = MP.PersonId

WHERE
	LP.LayoutId = @LayoutId AND
	MP.CancellationTime IS NULL AND
	P.EmailAddresses IS NOT NULL
RETURN 0
