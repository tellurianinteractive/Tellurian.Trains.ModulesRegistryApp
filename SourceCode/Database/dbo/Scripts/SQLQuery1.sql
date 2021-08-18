SELECT
	--CCS.LayoutId,
	--CCS.Id AS SupplierId,
	--CCS.IsInternal AS IsInternalSupplier,
	--CCS.StationId AS OriginStationId,
	CCS.StationName AS OriginStationName,
	--CCS.StationSignature AS OriginStationSignature,
	CCS.CustomerName AS Sender,
	--SC.Languages AS OriginLanguages,
	--SC.DomainSuffix AS OriginDomainSuffix,
	--CCS.BackColor,
	--CCS.ForeColor,
	--CCC.Id AS ConsumerId,
	--CCC.IsInternal AS IsInternalConsumer,
	--CCC.StationId AS DestinationStationId,
	CCC.StationName AS DesinationStationName,
	--CCC.StationSignature AS DestinationStationSignature,
	CCC.CustomerName AS Receiver,
	--CC.Languages AS DestinationLanguages,
	--CC.DomainSuffix AS DestinationDomainSuffix,
	--CCC.BackColor,
	--CCC.ForeColor,
	--SOD.Flag AS SendingDayFlag,
	--COD.Flag AS ReceivingDayFlag,
	CCC.SpecialCargoName,
	--CCC.PackageUnitId,
	--C.DefaultClasses,
	--CCC.SpecificWagonClass,
	--CCS.QuantityUnitId,
	CCS.Quantity,
	CU.FullName AS QuanityUnitResourceName,
	--COALESCE(CCS.PackageUnitId, CCC.PackageUnitId) AS PackageUnitId,
	C.NHMCode,
	C.DA,
	C.DE,
	C.EN,
	C.NL,
	C.NO,
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
	CargoUnit AS CU ON CU.Id = CCS.QuantityUnitId 
WHERE 
	(CCS.IsSupply <> 0 AND CCC.IsSupply = 0) AND
	((CCS.IsInternational <> 0 AND CCC.IsInternational <> 0) OR (CCS.CountryId = CCC.CountryId)) AND
	((CCS.Id = 0 AND CCC.Id <> 0 AND CCC.IsInternal <>0) OR (CCC.Id = 0 AND CCS.Id <> 0 AND CCS.IsInternal <> 0) OR (CCS.Id <> 0 AND CCC.Id <> 0)) AND
	(CCS.StationId <> CCC.StationId) AND
	((CCS.QuantityUnitId = CCC.QuantityUnitId)) AND
	(CCS.FromYear IS NULL OR CCS.FromYear <= 2014) AND (CCS.UptoYear IS NULL OR CCS.UptoYear >= 2021) AND
	(CCC.FromYear IS NULL OR CCC.FromYear <= 2014) AND (CCC.UptoYear IS NULL OR CCC.UptoYear >= 2021) AND
	((CCS.LayoutId = 1 AND CCC.LayoutId = 1) OR (CCS.LayoutId = 1 AND CCC.LayoutId = -1) OR (CCC.LayoutId = 1 AND CCS.LayoutId = -1))
	--AND (C.EN <> 'Unspecified' OR CCC.PackageUnitId > 0)

	-- AND CCC.StationId = 40
ORDER BY 
	CCS.StationName, CCC.StationName