CREATE VIEW [dbo].[ModuleOwnerNames]
	AS 
SELECT 
	MO.ModuleId,
	M.StationId,
	STRING_AGG(FirstName + ' ' + LastName, ', ') AS [Names]
FROM 
	ModuleOwnership AS MO INNER JOIN 
	Person AS P ON MO.PersonId = P.Id INNER JOIN 
	Module M ON M.Id = MO.ModuleId
WHERE 
	MO.OwnedShare > 0 
GROUP BY 
	MO.ModuleId, M.StationId

