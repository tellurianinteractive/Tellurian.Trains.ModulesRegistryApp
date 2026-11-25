CREATE VIEW [dbo].[StationStandard] AS 
-- The scales and standards of a station's modules. A station's theme is its primary module's (Module.PrimaryThemeId).
SELECT DISTINCT S.Id, M.ScaleId, M.StandardId
FROM 
	[Station] AS S
	INNER JOIN [Module] AS M ON M.StationId = S.Id
 