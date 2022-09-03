CREATE VIEW [dbo].[StationScale] AS 
SELECT DISTINCT S.Id, M.ScaleId
FROM 
	[Station] AS S
	INNER JOIN [Module] AS M ON M.StationId = S.Id
