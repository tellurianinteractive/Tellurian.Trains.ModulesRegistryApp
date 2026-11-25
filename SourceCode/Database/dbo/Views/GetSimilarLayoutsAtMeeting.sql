CREATE FUNCTION [dbo].[GetSimilarLayoutsAtMeeting]
(
	@MeetingId INT,
	@MainThemeId INT,
	@ScaleId INT
)
RETURNS TABLE AS RETURN
(
	SELECT DISTINCT 
		L.Id
	FROM 
		Layout AS L INNER JOIN 
		LayoutParticipant AS LP ON LP.LayoutId = L.Id INNER JOIN 
		LayoutModule AS LM ON LM.LayoutParticipantId = LP.Id  INNER JOIN 
		Module AS M ON M.Id = LM.ModuleId
	WHERE 
		L.MeetingId = @MeetingId AND 
		L.ThemeId = @MainThemeId AND 
		M.ScaleId = @ScaleId
)
