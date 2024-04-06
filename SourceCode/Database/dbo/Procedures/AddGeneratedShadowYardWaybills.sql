CREATE PROCEDURE [dbo].[AddGeneratedShadowYardWaybills]
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
			ShadowYardCustomerCargo AS OTHER ON ME.StationCustomerCargoId = OTHER.StationCustomerCargoId LEFT JOIN
			StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId 
		WHERE
		ME.StationCustomerId = @StationCustomerId AND
		OTHER.IsShadowYard <> 0 AND
		ME.CountryId = OTHER.CountryId AND
		SCW.OtherExternalCustomerCargoId IS NULL AND
		SCW.OtherStationCustomerCargoId IS NULL AND
		SCW.Id IS NOT NULL
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
	SELECT DISTINCT
		ME.StationCustomerId AS [StationCustomerId],
		ME.StationCustomerCargoId AS [StationCustomerCargoId],
		NULL AS [OtherStationCustomerCargoId],
		NULL AS [OtherExternalCustomerCargoId],
		ME.RegionId AS [OtherRegionId],
		ME.OperatingDayId AS OperatingDayId,
		0 AS [IsManuallyCreated],
		0 AS [HasEmptyReturn],
		CASE
			WHEN (ME.NHMCode = 0) THEN 1
			ELSE 0
		END AS [HasSameCargoReturn],
		0 AS [HideLoadingTimes],
		0 AS [HideUnloadingTimes],
		0 AS [PrintPerOperatingDay],
		1 AS [PrintCount],
		0 AS [SequenceNumber]
	FROM
		ModuleCustomerCargo AS ME INNER JOIN
		ShadowYardCustomerCargo AS OTHER ON ME.CargoId = OTHER.CargoId LEFT JOIN
		StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId 
	WHERE
		ME.NHMCode = 0 AND
		ME.StationCustomerId = @StationCustomerId
		AND ME.StationId <> OTHER.StationId
		--AND ME.StationCustomerId <> OTHER.StationCustomerId
		AND ME.IsSupply <> OTHER.IsSupply
		-- AND ME.QuantityUnitId = OTHER.QuantityUnitId
		AND ME.MainTheme = OTHER.MainTheme
		AND (ME.ScaleId = OTHER.ScaleId OR ME.ScaleId = 0 OR OTHER.ScaleId = 0)
		AND (ME.CountryId = OTHER.CountryId) 
		AND (ME.FromYear IS NULL OR OTHER.UptoYear IS NULL OR ME.FromYear <= OTHER.UptoYear )
		AND (ME.UptoYear IS NULL OR OTHER.FromYear IS NULL OR ME.UptoYear >= OTHER.FromYear )
		AND SCW.Id IS NULL
END