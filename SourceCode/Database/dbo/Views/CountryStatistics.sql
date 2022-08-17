CREATE VIEW [dbo].[CountryStatistics] AS
SELECT Modules.EnglishName, Modules.DomainSuffix, COALESCE(ModulesCount, 0) AS ModulesCount, StationsCount, StationCustomersCount, ExternalStationsCount, ExternalCustomersCount, ActiveUsersCount FROM
( 
	SELECT
		EnglishName, DomainSuffix, COUNT(Id) AS [ModulesCount]
	FROM
		(	SELECT 
				C.EnglishName, C.DomainSuffix, M.Id
			FROM 
				Country AS C 
				LEFT JOIN Person AS P ON P.CountryId = C.Id
				LEFT JOIN ModuleOwnership AS MO ON MO.PersonId = P.Id
				LEFT JOIN Module AS M ON M.Id = MO.ModuleId
			WHERE
				C.IsFullySupported <> 0
			UNION
			SELECT 
				C.EnglishName, C.DomainSuffix, M.Id
			FROM 
				Country AS C 
				LEFT JOIN [Group] AS G ON G.CountryId = C.Id
				LEFT JOIN ModuleOwnership AS MO ON MO.GroupId = G.Id
				LEFT JOIN Module AS M ON M.Id = MO.ModuleId
			WHERE
				C.IsFullySupported <> 0
		) AS M
	GROUP BY
		M.EnglishName, M.DomainSuffix
) AS Modules
LEFT JOIN
(
	SELECT 
		COALESCE(SC.EnglishName, MC.EnglishName) AS EnglishName, COUNT(S.Id) AS [StationsCount]
	FROM 
		Station AS S
		LEFT JOIN Region AS R ON R.Id = S.RegionId
		LEFT JOIN Country AS SC ON SC.Id = R.CountryId
		INNER JOIN Module AS M ON S.Id = M.StationId
		INNER JOIN ModuleOwnership AS MO ON MO.ModuleId = M.Id
		INNER JOIN Person AS P ON P.Id = MO.PersonId
		INNER JOIN Country AS MC ON MC.Id = P.CountryId
	GROUP BY
		COALESCE(SC.EnglishName, MC.EnglishName) 
)
AS Stations ON Stations.EnglishName = Modules.EnglishName
LEFT JOIN
(
	SELECT
		COALESCE(RC.EnglishName, PC.EnglishName) AS EnglishName, COUNT(S.Id) AS [StationCustomersCount]
	FROM 
		StationCustomer AS SC
		INNER JOIN Station AS S ON SC.StationId = S.Id
		LEFT JOIN Region AS R ON R.Id = S.RegionId
		LEFT JOIN Country AS RC ON RC.Id = R.CountryId
		INNER JOIN Module AS M ON S.Id = M.StationId
		INNER JOIN ModuleOwnership AS MO ON MO.ModuleId = M.Id
		INNER JOIN Person AS P ON P.Id = MO.PersonId
		INNER JOIN Country AS PC ON PC.Id = P.CountryId
	GROUP BY
		COALESCE(RC.EnglishName, PC.EnglishName) 

)
AS Customers  ON Customers.EnglishName = Modules.EnglishName
LEFT JOIN
(
	SELECT 
		C.EnglishName, Count(ES.Id) AS ExternalStationsCount
	FROM 
		ExternalStation AS ES
		INNER JOIN Region AS R ON R.Id = ES.RegionId
		INNER JOIN Country AS C ON C.Id = R.CountryId
	GROUP BY
		C.EnglishName
)
AS ExternalStations ON ExternalStations.EnglishName = Modules.EnglishName
LEFT JOIN
(
	SELECT 
		C.EnglishName, Count(ESC.Id) AS ExternalCustomersCount
	FROM 
		ExternalStationCustomer AS ESC
		INNER JOIN ExternalStation AS ES ON ESC.ExternalStationId = ES.Id
		INNER JOIN Region AS R ON R.Id = ES.RegionId
		INNER JOIN Country AS C ON C.Id = R.CountryId
	GROUP BY
		C.EnglishName

)
AS ExternalCustomers ON ExternalCustomers.EnglishName = Modules.EnglishName
LEFT JOIN
(
	SELECT 
		C.EnglishName, Count(U.Id) AS ActiveUsersCount
	FROM
		[User] AS U INNER JOIN [Person] AS P ON U.Id = P.UserId
		INNER JOIN [Country] AS C ON C.Id = P.CountryId
	WHERE U.LastSignInTime IS NOT NULL
	GROUP BY
		C.EnglishName
) AS ActiveUsers ON ActiveUsers.EnglishName = Modules.EnglishName

