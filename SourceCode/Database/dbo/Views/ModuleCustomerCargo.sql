CREATE VIEW [dbo].[ModuleCustomerCargo]
	AS SELECT
	SCC.Id,
	SCC.StationCustomerId AS StationCustomerId,
	SCC.OperatingDayId,
	S.Id AS StationId,
	S.FullName AS StationName,
	S.Signature AS StationSignature,
	SCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	0 AS IsInternal,
	0 AS IsShadowYard,
	R.CountryId,
	C.Languages,
	C.DomainSuffix,
	R.Id AS RegionId,
	R.BackColor,
	R.ForeColor,
	SC.CustomerName,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	OD.Flag,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	CRT.ShortName AS ReadyTime,
	COALESCE(SCC.TrackOrArea, SC.TrackOrArea) AS TrackOrArea,
	CASE
		WHEN SCC.TrackOrAreaColor IS NOT NULL AND SCC.TrackOrAreaColor <> '#ffffff' THEN SCC.TrackOrAreaColor
		ELSE SC.TrackOrAreaColor
	END AS TrackOrAreaColor,
	SCC.EmptyReturn,
	SCC.MatchReturn
FROM 
	[Station] AS S INNER JOIN Region AS R ON R.Id = S.RegionId
	INNER JOIN [Country] AS C ON C.Id = R.CountryId
	INNER JOIN [StationCustomer] AS SC ON SC.StationId = S.Id
	INNER JOIN [StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id
	INNER JOIN [CargoDirection] AS CD ON SCC.DirectionId = CD.Id
	INNER JOIN [CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId
	INNER JOIN [OperatingDay] AS OD ON OD.Id = SCC.OperatingDayId
