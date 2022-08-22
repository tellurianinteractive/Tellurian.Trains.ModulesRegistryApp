CREATE VIEW [dbo].[ExternalCustomerCargo]
	AS SELECT 
	ESCC.Id AS Id,
	ESCC.ExternalStationCustomerId AS StationCustomerId,
	ESCC.OperatingDayId,
	ES.[Id] AS StationId,
	ES.FullName AS StationName,
	ES.Signature AS StationSignature,
	ESCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	0 AS IsInternal,
	0 AS IsShadowYard,
	C.DefaultClasses,
	R.CountryId,
	R.Id AS RegionId,
	R.BackColor,
	R.ForeColor,
	ESC.CustomerName,
	COALESCE(ESCC.FromYear, ESC.OpenedYear) AS FromYear,
	COALESCE(ESCC.UptoYear, ESC.ClosedYear) AS UptoYear,
	ESCC.Quantity,
	ESCC.QuantityUnitId,
	ESCC.PackageUnitId,
	ESCC.SpecificWagonClass,
	ESCC.SpecialCargoName,
	NULL AS ReadyTime,
	0 AS ReadyTimeIsSpecifiedInLayout,
	NULL AS TrackOrArea,
	'#ffffff' AS TrackOrAreaColor,
	0 AS EmptyReturn,
	0 AS MatchReturn
FROM [ExternalStation] AS ES 
	INNER JOIN [Region] AS R ON ES.RegionId = R.Id 
	INNER JOIN [ExternalStationCustomer] AS ESC ON ESC.ExternalStationId = ES.Id
	INNER JOIN [ExternalStationCustomerCargo] AS ESCC ON ESCC.ExternalStationCustomerId = ESC.Id
	INNER JOIN [Cargo] AS C ON ESCC.CargoId = C.Id
	INNER JOIN [CargoDirection] AS CD ON ESCC.DirectionId = CD.Id