CREATE PROCEDURE [dbo].[AddGeneratedModuleWaybills]
	@StationCustomerId INT
AS
BEGIN
	SET NOCOUNT ON
	INSERT INTO StationCustomerWaybill
	SELECT
		ME.StationCustomerId AS [StationCustomerId],
		ME.Id AS [StationCustomerCargoId],
		OTHER.Id AS [OtherCustomerCargoId],
		OTHER.RegionId AS [OtherRegionId],
		ME.OperatingDayId AS OperatingDayId,
		0 AS [IsManuallyCreated],
		0 AS [HasEmptyReturn],
		0 AS [HideLoadingTimes],
		0 AS [HideUnloadingTimes],
		0 AS [PrintPerOperatingDay],
		1 AS [PrintCount],
		0 AS [SequenceNumber]
	FROM
		ModuleCustomerCargo AS ME INNER JOIN
		ModuleCustomerCargo AS OTHER ON ME.CargoId = OTHER.CargoId LEFT JOIN
		StationCustomerWaybill AS SCW ON ME.Id = SCW.StationCustomerCargoId AND OTHER.Id = SCW.OtherCustomerCargoId
	WHERE
		ME.StationCustomerId = @StationCustomerId
		AND ME.StationCustomerId <> OTHER.StationCustomerId
		AND ME.IsSupply <> OTHER.IsSupply
		AND ME.QuantityUnitId = OTHER.QuantityUnitId
		AND (ME.CountryId = OTHER.CountryId OR ME.IsInternational <> 0 AND OTHER.IsInternational <>0) 
		AND (ME.FromYear IS NULL OR OTHER.UptoYear IS NULL OR ME.FromYear <= OTHER.UptoYear )
		AND (ME.UptoYear IS NULL OR OTHER.FromYear IS NULL OR ME.UptoYear >= OTHER.FromYear )
		AND SCW.Id IS NULL
END