CREATE PROCEDURE [dbo].[AddGeneratedHubWaybills]
	@StationCustomerId INT
AS
-- The columns in this stored procedures must match table StationCustomerWaybill
BEGIN
	SET NOCOUNT ON;

	WITH UpdatedableWaybill( Id, OperatingDayId, RegionId) AS
	(
		SELECT 
			SCW.Id, ME.OperatingDayId, OTHER.RegionId
		FROM 
			ModuleCustomerCargo AS ME INNER JOIN
			ModuleCustomerCargo AS OTHER ON ME.CargoId = OTHER.CargoId LEFT JOIN
			StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId AND 
			OTHER.StationCustomerCargoId = SCW.OtherStationCustomerCargoId
		WHERE
			ME.NHMCode = 0 AND  ME.StationCustomerId = @StationCustomerId AND SCW.Id IS NOT NULL
	)
	UPDATE StationCustomerWaybill 
		SET 
			OperatingDayId = UW.OperatingDayId,
			OtherRegionId = UW.RegionId
		FROM 
			UpdatedableWaybill UW
		WHERE
			StationCustomerWaybill.Id = UW.Id
	
	INSERT INTO StationCustomerWaybill
	SELECT
		ME.StationCustomerId AS [StationCustomerId],
		ME.StationCustomerCargoId AS [StationCustomerCargoId],
		NULL AS [OtherStationCustomerCargoId],
		NULL AS [OtherExternalCustomerCargoId],
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
		StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId AND OTHER.StationCustomerCargoId = SCW.OtherStationCustomerCargoId
	WHERE
		OTHER.IsCargoHub <> 0
		AND ME.NHMCode = 0 
		AND ME.StationCustomerId = @StationCustomerId
		AND ME.StationCustomerId <> OTHER.StationCustomerId
		AND ME.IsSupply <> OTHER.IsSupply AND ME.IsSupply = 0
		--AND ME.QuantityUnitId = OTHER.QuantityUnitId -- Is not used, because it causes too few matches.
		AND ME.StationId <> OTHER.StationId
		AND ME.MainTheme = OTHER.MainTheme
		AND (ME.ScaleId = OTHER.ScaleId OR ME.ScaleId = 0 OR OTHER.ScaleId = 0)
		AND (ME.CountryId = OTHER.CountryId) 
		AND (ME.FromYear IS NULL OR OTHER.UptoYear IS NULL OR ME.FromYear <= OTHER.UptoYear )
		AND (ME.UptoYear IS NULL OR OTHER.FromYear IS NULL OR ME.UptoYear >= OTHER.FromYear )
		AND SCW.Id IS NULL
END