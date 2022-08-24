CREATE VIEW [dbo].[ModuleCustomerCargo] AS 
SELECT
	SCC.StationCustomerId AS StationCustomerId,
	SCC.Id AS StationCustomerCargoId,
	SCC.OperatingDayId,
	SCC.CargoId,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	SCC.EmptyReturn,
	SCC.MatchReturn,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	COALESCE(SCC.TrackOrArea, SC.TrackOrArea) AS TrackOrArea,
	CASE
		WHEN SCC.TrackOrAreaColor IS NOT NULL AND SCC.TrackOrAreaColor <> '#ffffff' THEN SCC.TrackOrAreaColor
		ELSE SC.TrackOrAreaColor
	END AS TrackOrAreaColor,
	SC.Id AS CustomerId,
	SC.CustomerName,
	CD.IsSupply,
	CD.IsInternational,
	CRT.ShortName AS ReadyTime,
	CU.FullName AS QuantityUnitName,
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
	CAST (1 AS BIT) AS IsModuleStation,
	CAST (0 AS BIT) AS IsShadowYard

FROM 
	[Station] AS S INNER JOIN Region AS R ON R.Id = S.RegionId
	INNER JOIN [Country] AS C ON C.Id = R.CountryId
	INNER JOIN [StationCustomer] AS SC ON SC.StationId = S.Id
	INNER JOIN [StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id
	INNER JOIN [CargoPackagingUnit] AS CPU ON CPU.Id = SCC.PackageUnitId
	INNER JOIN [CargoDirection] AS CD ON SCC.DirectionId = CD.Id
	INNER JOIN [CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId
	INNER JOIN [CargoUnit] AS CU ON CU.Id = SCC.QuantityUnitId
	INNER JOIN [OperatingDay] AS OD ON OD.Id = SCC.OperatingDayId
