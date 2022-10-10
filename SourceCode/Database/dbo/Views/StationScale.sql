CREATE VIEW [dbo].[StationStandard] AS 
SELECT DISTINCT S.Id, M.ScaleId, M.StandardId, MS.MainTheme
FROM 
	[Station] AS S
	INNER JOIN [Module] AS M ON M.StationId = S.Id
	INNER JOIN [ModuleStandard] AS MS ON MS.Id = M.StandardId
 