CREATE VIEW [dbo].[LayoutCustomerCargo] AS 	
SELECT -- Internal station customer cargo
	LP.LayoutId,
	SCC.Id,
	SCC.OperatingDayId,
	S.[Id] AS StationId,
	COALESCE(LS.OtherName, S.FullName) AS StationName,
	COALESCE(LS.OtherSignature, S.Signature) AS StationSignature,
	SCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	CAST (1 AS BIT) AS IsInternal,
	CAST (0 AS BIT) AS IsShadowYard,
	C.DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	'#FFFFFF' AS BackColor,
	'#000000' AS ForeColor,
	SC.CustomerName,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	CRT.ShortName AS ReadyTime,
	CRT.IsSpecifiedInLayout AS ReadyTimeIsSpecifiedInLayout,
	COALESCE(SCC.TrackOrArea, SC.TrackOrArea) AS TrackOrArea,
	CASE
		WHEN SCC.TrackOrAreaColor IS NOT NULL AND SCC.TrackOrAreaColor <> '#FFFFFF' THEN SCC.TrackOrAreaColor
		ELSE SC.TrackOrAreaColor
	END AS TrackOrAreaColor,
	SCC.EmptyReturn,
	SCC.MatchReturn
FROM 
	[Station] AS S INNER JOIN 
	[Region] AS R ON S.RegionId = R.Id INNER JOIN 
	[StationCustomer] AS SC ON SC.StationId = S.Id INNER JOIN 
	[StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id INNER JOIN 
	[Cargo] AS C ON SCC.CargoId = C.Id INNER JOIN 
	[CargoDirection] AS CD ON SCC.DirectionId = CD.Id INNER JOIN 
	[CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId INNER JOIN 
	[LayoutStation] AS LS ON LS.StationId = S.Id INNER JOIN 
	[LayoutParticipant] LP ON LS.LayoutParticipantId = LP.Id
UNION SELECT -- Supply from shadow stations
	LP.LayoutId,
	0 AS Id,
	8 AS OperatingDayId,
	LS.StationId,
	COALESCE(LS.OtherName, S.FullName) AS StationName,
	COALESCE(LS.OtherSignature, S.Signature) AS StationSignature,
	C.Id AS GargoId,
	CAST (1 AS BIT) AS IsSupply,
	CAST (1 AS BIT) AS IsInternational,
	CAST (1 AS BIT) AS IsInternal,
	CAST (1 AS BIT) AS IsShadowYard,
	'' AS DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	'#FFFFFF' AS BackColor,
	'#000000' AS ForeColor,
	'ImportAgent' AS CustomerName,
	NULL AS FromYear,
	NULL AS UptoYear,
	10 AS Quantity,
	4 AS QuantityUnitId,
	0 AS PackageUnitId,
	NULL AS SpecificWagonClass,
	NULL AS SpecialCargoName,
	NULL AS ReadyTime,
	CAST (0 AS BIT) AS ReadyTimeIsSpecifiedInLayout,
	NULL AS TrackOrArea,
	'#FFFFFF' AS TrackOrAreaColor,
	CAST (0 AS BIT) AS EmptyReturn,
	CAST (0 AS BIT) AS MatchReturn
FROM 
	LayoutParticipant LP INNER JOIN 
	LayoutStation LS ON LS.LayoutParticipantId = LP.Id INNER JOIN
	Station S ON S.Id = LS.StationId INNER JOIN 
	LayoutStationRegion LSR ON LSR.LayoutStationId = LS.Id INNER JOIN
	Region R ON R.Id = LSR.RegionId INNER JOIN 
	Cargo AS C ON C.Id > 0
WHERE
	S.IsShadow <> 0

UNION SELECT -- Supply to shadow stations
	LP.LayoutId,
	0 AS Id,
	8 AS OperatingDayId,
	LS.StationId,
	COALESCE(LS.OtherName, S.FullName) AS StationName,
	COALESCE(LS.OtherSignature, S.Signature) AS StationSignature,
	C.Id AS GargoId,
	CAST (0 AS BIT) AS IsSupply,
	CAST (1 AS BIT) AS IsInternational,
	CAST (1 AS BIT) AS IsInternal,
	CAST (1 AS BIT) AS IsShadowYard,
	'' AS DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
	'#FFFFFF' AS BackColor,
	'#000000' AS ForeColor,
	'ExportAgent' AS CustomerName,
	NULL AS FromYear,
	NULL AS UptoYear,
	10 AS Quantity,
	4 AS QuantityUnitId,
	0 AS PackageUnitId,
	NULL AS SpecificWagonClass,
	NULL AS SpecialCargoName,
	NULL AS ReadyTime,
	CAST (0 AS BIT) AS ReadyTimeIsSpecifiedInLayout,
	NULL AS TrackOrArea,
	'#FFFFFF' AS TrackOrAreaColor,
	CAST (0 AS BIT) AS EmptyReturn,
	CAST (0 AS BIT) AS MatchReturn
FROM 
	[LayoutParticipant] LP INNER JOIN 
	[LayoutStation] LS ON LS.LayoutParticipantId = LP.Id INNER JOIN
	[Station] S ON S.Id = LS.StationId INNER JOIN 
	[LayoutStationRegion] LSR ON LSR.LayoutStationId = LS.Id INNER JOIN
	[Region] R ON R.Id = LSR.RegionId INNER JOIN 
	[Cargo] AS C ON C.Id > 0
WHERE
	S.IsShadow <> 0

