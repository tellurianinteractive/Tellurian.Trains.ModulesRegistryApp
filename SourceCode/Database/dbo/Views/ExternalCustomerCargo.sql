CREATE VIEW [dbo].[ExternalCustomerCargo] AS 
SELECT
	SCC.ExternalStationCustomerId AS StationCustomerId,
	SCC.Id AS StationCustomerCargoId,
	SCC.OperatingDayId,
	SCC.CargoId,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	CD.IsSupply AS IsEmptyReturn,
	CAST (0 AS BIT) AS MatchReturn,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	'' AS CustomerTrackOrArea,
	'' AS CustomerTrackOrAreaColor,
	'' AS CargoTrackOrArea,
	'' AS CargoTrackOrAreaColor,
	SC.Id AS CustomerId,
	SC.CustomerName,
	CD.IsSupply,
	CD.IsInternational,
	'' AS ReadyTime,
	CASE
		WHEN SCC.Quantity <=1 THEN CU.SingularResourceCode
		ELSE CU.PluralResourceCode
	END AS QuantityUnitResourceCode,
	CASE
		WHEN CPU.Id=3 AND SCC.Quantity <=1 THEN CPU.SingularResourceCode
		ELSE CPU.PluralResourceCode
	END AS PackagingUnit,
	S.Id AS StationId,
	S.FullName AS StationName,
	S.Signature AS StationSignature,
	R.Id AS RegionId,
	R.CountryId,
	R.BackColor,
	R.ForeColor,
	C.Languages,
	C.DomainSuffix,
	OD.Flag AS OperatingDayFlag,
	OD.DisplayOrder AS OperatingDayDisplayOrder,
	CAST (0 AS BIT) AS IsModuleStation,
	CAST (0 AS BIT) AS IsShadowYard,
	0 AS ScaleId 

FROM 
	[ExternalStation] AS S INNER JOIN Region AS R ON R.Id = S.RegionId
	INNER JOIN [Country] AS C ON C.Id = R.CountryId
	INNER JOIN [ExternalStationCustomer] AS SC ON SC.ExternalStationId = S.Id
	INNER JOIN [ExternalStationCustomerCargo] AS SCC ON SCC.ExternalStationCustomerId = SC.Id
	INNER JOIN [CargoPackagingUnit] AS CPU ON CPU.Id = SCC.PackageUnitId
	INNER JOIN [CargoDirection] AS CD ON SCC.DirectionId = CD.Id
	INNER JOIN [CargoUnit] AS CU ON CU.Id = SCC.QuantityUnitId
	INNER JOIN [OperatingDay] AS OD ON OD.Id = SCC.OperatingDayId
