CREATE VIEW [dbo].[ShadowYardCustomerCargo] AS 
SELECT
	0 AS StationCustomerId,
	0 AS StationCustomerCargoId,
	8 AS OperatingDayId,
	CO.Id AS CargoId,
	CASE
		WHEN CU.IsBearer <> 0 THEN 1
		ELSE SCC.Quantity
	END AS Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	SCC.EmptyReturn,
	SCC.MatchReturn,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	SC.TrackOrArea AS CustomerTrackOrArea,
	SC.TrackOrAreaColor AS CustomerTrackOrAreaColor,
	SCC.TrackOrArea AS CargoTrackOrArea,
	SCC.TrackOrAreaColor AS CargoTrackOrAreaColor,
	0 AS CustomerId,
	'ShadowYard' AS CustomerName,
	CD.IsSupply,
	CD.IsInternational,
	CRT.ShortName AS ReadyTime,
	CASE
		WHEN SCC.Quantity <=1 OR CU.IsBearer <> 0 THEN CU.SingularResourceCode
		ELSE CU.PluralResourceCode
	END AS QuantityUnitResourceCode,	
	CU.Designation AS QuantityShortUnit,
	CASE
		WHEN CPU.Id=3 AND SCC.Quantity <=1 THEN CPU.SingularResourceCode
		ELSE CPU.PluralResourceCode
	END AS PackagingUnit,
	CPU.PrepositionResourceCode AS PackagingPrepositionResourceCode,
	0 AS StationId,
	COALESCE(RES.FullName, R.LocalName) AS StationName,
	'' AS InternationalStationName,
	RES.Signature AS StationSignature,
	R.Id AS RegionId,
	R.CountryId,
	R.BackColor,
	R.ForeColor,
	R.IsCargoHub,
	C.Languages,
	C.DomainSuffix,
	OD.Flag AS OperatingDayFlag,
	OD.DisplayOrder AS OperatingDayDisplayOrder,
	CAST (0 AS BIT) AS IsModuleStation,
	CAST (1 AS BIT) AS IsShadowYard,
	0 AS ScaleId,
	0 AS StandardId,
	COALESCE(SS.MainTheme, 'EUROPE') AS MainTheme,
	CO.NHMCode
FROM 
	[Station] AS S INNER JOIN Region AS R ON R.Id = S.RegionId
	INNER JOIN [StationStandard] SS ON SS.Id = S.Id 
	INNER JOIN [Country] AS C ON C.Id = R.CountryId
	INNER JOIN [StationCustomer] AS SC ON SC.StationId = S.Id
	INNER JOIN [StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id
	INNER JOIN [CargoPackagingUnit] AS CPU ON CPU.Id = SCC.PackageUnitId
	INNER JOIN [CargoDirection] AS CD ON SCC.DirectionId = CD.Id
	INNER JOIN [CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId
	INNER JOIN [CargoUnit] AS CU ON CU.Id = SCC.QuantityUnitId
	INNER JOIN [OperatingDay] AS OD ON OD.Id = SCC.OperatingDayId
	INNER JOIN [Cargo] AS CO ON CO.Id = SCC.CargoId
	LEFT JOIN [ExternalStation] AS RES ON RES.Id = R.RepresentativeExternalStationId
WHERE 
	CO.NHMCode = 0
