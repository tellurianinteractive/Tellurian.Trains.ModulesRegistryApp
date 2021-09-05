CREATE PROCEDURE [dbo].[GetWaybills]
	@LayoutId INT,
	@StationId INT = NULL,
	@MatchShadowYard BIT = 0,
	@Sending BIT = 1,
	@Receiving BIT = 1
AS
BEGIN
	DECLARE @FromYear AS INT, @UptoYear AS INT

	SELECT @FromYear = L.FirstYear, @UptoYear = L.LastYear
	FROM
		Layout AS L
	WHERE
		L.Id = @LayoutId

	SELECT
		CCS.LayoutId,
		CCS.StationName AS OriginStationName,
		CCS.CustomerName AS Sender,
		SC.Languages AS OriginLanguages,
		SC.DomainSuffix AS OriginDomainSuffix,
		CCS.BackColor AS OriginBackColor,
		CCS.ForeColor AS OriginForeColor,
		CCS.IsInternal AS OriginIsInternal,
		CCS.ReadyTime AS OriginReadyTime,
		CCS.ReadyTimeIsSpecifiedInLayout AS OriginReadyTimeIsSpecifiedInLayout,
		CCC.StationName AS DesinationStationName,
		CCC.CustomerName AS Receiver,
		CC.Languages AS DestinationLanguages,
		CC.DomainSuffix AS DestinationDomainSuffix,
		CCC.BackColor AS DestinationBackColor,
		CCC.ForeColor AS DestinationForeColor,
		CCC.IsInternal AS DestinationIsInternal,
		CCC.ReadyTime AS DestinationReadyTime,
		CCC.ReadyTimeIsSpecifiedInLayout AS DestinationReadyTimeIsSpecifiedInLayout,
		SOD.Flag AS SendingDayFlag,
		COD.Flag AS ReceivingDayFlag,
		CCC.SpecialCargoName,
		C.DefaultClasses,
		CCC.SpecificWagonClass,
		CCS.Quantity,
		CU.FullName AS QuanityUnitResourceName,
		CCS.PackageUnitId AS OriginPackageUnitId,
		CCC.PackageUnitId AS DestinationPackageUnitId, 
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
		(@StationId IS NULL OR @StationId = CCS.StationId OR @StationId = CCC.StationId) AND
		((@Sending <> 0 OR CCC.StationId = @StationId) OR (@Receiving <> 0 OR CCS.StationId=@StationId)) AND
		(CCS.QuantityUnitId = CCC.QuantityUnitId) AND
		(CCS.FromYear IS NULL OR @UptoYear IS NULL OR CCS.FromYear <= @UptoYear) AND (CCS.UptoYear IS NULL OR @FromYear IS NULL OR CCS.UptoYear >= @FromYear) AND
		(CCC.FromYear IS NULL OR @UptoYear IS NULL OR CCC.FromYear <= @UptoYear) AND (CCC.UptoYear IS NULL OR @FromYear IS NULL OR CCC.UptoYear >= @FromYear) AND
		((CCS.LayoutId = @LayoutId AND CCC.LayoutId = @LayoutId) OR (CCS.LayoutId= @LayoutId AND CCC.LayoutId = -1) OR (CCC.LayoutId = @LayoutId AND CCS.LayoutId = -1)) AND
		((@MatchShadowYard <> 0) OR (CCS.IsShadowYard = 0 AND CCC.IsShadowYard = 0))
	ORDER BY
		CCC.StationName, 
		CCC.CustomerName,
		CCS.StationName,
		CCS.CustomerName

	RETURN 0
END
