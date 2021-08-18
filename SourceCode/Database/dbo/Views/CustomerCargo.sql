CREATE VIEW [dbo].[CustomerCargo]
AS 
SELECT -- Internal station customer cargo
	LS.LayoutId,
	SCC.Id,
	SCC.OperatingDayId,
	S.[Id] AS StationId,
	S.FullName AS StationName,
	S.Signature AS StationSignature,
	SCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	1 AS IsInternal,
	0 AS IsShadowYard,
	C.DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	'#FFFFFF' AS BackColor,
	'#000000' AS ForeColor,
	SC.CustomerName,
	SCC.FromYear,
	SCC.UptoYear,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName
FROM [Station] AS S 
	INNER JOIN [Region] AS R ON S.RegionId = R.Id 
	INNER JOIN [StationCustomer] AS SC ON SC.StationId = S.Id
	INNER JOIN [StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id
	INNER JOIN [Cargo] AS C ON SCC.CargoId = C.Id
	INNER JOIN [CargoDirection] AS CD ON SCC.DirectionId = CD.Id
	LEFT JOIN [LayoutStation] AS LS ON LS.StationId = S.Id
UNION
SELECT -- External station customer cargo
	-1 AS LayoutId,
	-ESCC.Id AS Id,
	ESCC.OperatingDayId,
	-ES.[Id] AS StationId,
	ES.FullName AS StationName,
	ES.Signature AS StationSignature,
	ESCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	0 AS IsInternal,
	0 AS IsShadowYard,
	C.DefaultClasses,
	R.CountryId,
	R.BackColor,
	R.ForeColor,
	ESC.CustomerName,
	COALESCE(ESCC.FromYear, ESC.OpenedYear) AS FromYear,
	COALESCE(ESCC.UptoYear, ESC.ClosedYear) AS UptoYear,
	ESCC.Quantity,
	ESCC.QuantityUnitId,
	ESCC.PackageUnitId,
	ESCC.SpecificWagonClass,
	ESCC.SpecialCargoName
FROM [ExternalStation] AS ES 
	INNER JOIN [Region] AS R ON ES.RegionId = R.Id 
	INNER JOIN [ExternalStationCustomer] AS ESC ON ESC.ExternalStationId = ES.Id
	INNER JOIN [ExternalStationCustomerCargo] AS ESCC ON ESCC.ExternalStationCustomerId = ESC.Id
	INNER JOIN [Cargo] AS C ON ESCC.CargoId = C.Id
	INNER JOIN [CargoDirection] AS CD ON ESCC.DirectionId = CD.Id
UNION

SELECT -- Shadow yard suppliers
	LS.LayoutId,
	0 AS Id,
	8 AS OperatingDayId,
	LS.StationId,
	S.FullName AS StationName,
	S.Signature AS StationSignature,
	C.Id AS GargoId,
	1 AS IsSupply,
	1 AS IsInternational,
	1 AS IsInternal,
	1 AS IsShadowYard,
	'' AS DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	COALESCE(R.BackColor, '#FFFFFF') AS BackColor,
	COALESCE(R.ForeColor, '#000000') AS ForeColor,
	'SwitchYard' AS CustomerName,
	NULL AS FromYear,
	NULL AS UptoYear,
	10 AS Quantity,
	4 AS QuantityUnitId,
	0 AS PackageUnitId,
	NULL AS SpecificWagonClass,
	NULL AS SpecialCargoName
FROM 
	LayoutStation LS
	INNER JOIN Station S ON S.Id = LS.StationId
	LEFT JOIN Region R ON R.Id = S.RegionId
	JOIN Cargo AS C ON C.Id > 0
WHERE
	S.IsShadow <> 0
UNION
SELECT -- Shadow yard consumers
	LS.LayoutId,
	0 AS Id,
	8 AS OperatingDayId,
	LS.StationId,
	S.FullName AS StationName,
	S.Signature AS StationSignature,
	C.Id AS GargoId,
	0 AS IsSupply,
	1 AS IsInternational,
	1 AS IsInternal,
	1 AS IsShadowYard,
	'' AS DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	COALESCE(R.BackColor, '#FFFFFF') AS BackColor,
	COALESCE(R.ForeColor, '#000000') AS ForeColor,
	'SwitchYard' AS CustomerName,
	NULL AS FromYear,
	NULL AS UptoYear,
	10 AS Quantity,
	4 AS QuantityUnitId,
	0 AS PackageUnitId,
	NULL AS SpecificWagonClass,
	NULL AS SpecialCargoName
FROM 
	LayoutStation LS
	INNER JOIN Station S ON S.Id = LS.StationId
	LEFT JOIN Region R ON R.Id = S.RegionId
	JOIN Cargo AS C ON C.Id > 0
WHERE
	S.IsShadow <> 0