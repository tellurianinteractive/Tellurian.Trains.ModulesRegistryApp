CREATE VIEW [dbo].[ModuleOwnerNames]
	AS 
SELECT
	X.ModuleId,
	X.StationId,
	STRING_AGG(X.Name, ', ') AS [Names]
FROM 
	(
	SELECT 
		MO.ModuleId,
		M.StationId,
		P.FirstName + ' ' + LastName AS [Name]
	FROM 
		[ModuleOwnership] AS MO INNER JOIN 
		[Person] AS P ON MO.PersonId = P.Id INNER JOIN 
		[Module] M ON M.Id = MO.ModuleId
	WHERE 
		MO.OwnedShare > 0 
	UNION
	SELECT 
		MO.ModuleId,
		M.StationId,
		G.ShortName AS [Name]
	FROM
		[ModuleOwnership] AS MO INNER JOIN
		[Group] AS G ON MO.GroupId = G.Id INNER JOIN
		[Module] AS M ON M.Id = MO.ModuleId
	WHERE 
		Mo.OwnedShare > 0
) AS X
GROUP BY
	X.ModuleId,
	X.StationId