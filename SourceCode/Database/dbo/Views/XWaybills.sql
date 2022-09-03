CREATE VIEW [dbo].[Waybills]
AS
SELECT
	CCS.LayoutId,
	CCS.ExcludeLayoutId,
	CCS.StationName AS OriginStationName,
	CCS.CustomerName AS Sender,
	SC.Languages AS OriginLanguages,
	SC.DomainSuffix AS OriginDomainSuffix,
	CCS.BackColor AS OriginBackColor,
	CCS.ForeColor AS OriginForeColor,
	CCC.StationName AS DesinationStationName,
	CCC.CustomerName AS Receiver,
	CC.Languages AS DestinationLanguages,
	CC.DomainSuffix AS DestinationDomainSuffix,
	CCC.BackColor AS DestinationBackColor,
	CCC.ForeColor AS DestinationForeColor,
	SOD.Flag AS SendingDayFlag,
	COD.Flag AS ReceivingDayFlag,
	CCC.SpecialCargoName,
	C.DefaultClasses,
	CCC.SpecificWagonClass,
	CCS.Quantity,
	CU.FullName AS QuanityUnitResourceName,
	COALESCE(CCC.PackageUnitId, CCS.PackageUnitId) AS PackageUnitId,
	C.NHMCode,
	C.DA,
	C.DE,
	C.EN,
	C.NL,
	C.NB,
	C.PL,
	C.SV
FROM
	CustomerCargo AS CCS INNER JOIN 
	CustomerCargo AS CCC ON CCC.CargoId = CCS.CargoId  INNER JOIN 
	Cargo AS C ON C.Id = CCS.CargoId INNER JOIN
	OperatingDay AS SOD ON SOD.Id = CCS.OperatingDayId INNER JOIN
	OperatingDay AS COD ON COD.Id = CCC.OperatingDayId INNER JOIN
	Country AS SC ON SC.Id = CCS.CountryId INNER JOIN
	Country AS CC ON CC.Id = CCC.CountryId INNER JOIN
	CargoUnit AS CU ON CU.Id = CCS.QuantityUnitId INNER JOIN
	Layout AS L ON L.Id = CCS.LayoutId
WHERE 
	(CCS.IsSupply <> 0 AND CCC.IsSupply = 0) AND
	((CCS.IsInternational <> 0 AND CCC.IsInternational <> 0) OR (CCS.CountryId = CCC.CountryId)) AND
	((CCS.Id = 0 AND CCC.Id <> 0 AND CCC.IsInternal <>0) OR (CCC.Id = 0 AND CCS.Id <> 0 AND CCS.IsInternal <> 0) OR (CCS.Id <> 0 AND CCC.Id <> 0)) AND
	(CCS.StationId <> CCC.StationId) AND
	(CCS.QuantityUnitId = CCC.QuantityUnitId) AND
	(CCS.FromYear IS NULL OR CCS.FromYear <= L.LastYear) AND (CCS.UptoYear IS NULL OR CCS.UptoYear >= L.FirstYear) AND
	(CCC.FromYear IS NULL OR CCC.FromYear <= L.LastYear) AND (CCC.UptoYear IS NULL OR CCC.UptoYear >= L.FirstYear) AND
	((CCS.LayoutId > 0 AND CCC.LayoutId > 0) OR (CCS.LayoutId > 0 AND CCC.LayoutId = -1) OR (CCC.LayoutId > 0 AND CCS.LayoutId = -1))
