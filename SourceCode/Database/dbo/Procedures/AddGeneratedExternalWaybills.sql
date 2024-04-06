CREATE PROCEDURE [dbo].[AddGeneratedExternalWaybills]
	@StationCustomerId INT
AS
-- The columns in this stored procedures must match table StationCustomerWaybill
BEGIN
	SET NOCOUNT ON;

	WITH UpdatedableWaybill (Id, OperatingDayId, RegionId, PrintCount) AS
	(
		SELECT 
			SCW.Id, ME.OperatingDayId, OTHER.RegionId, CASE WHEN ME.QuantityIsBearer <> 0 THEN ME.Quantity ELSE 1 END
		FROM 
			ModuleCustomerCargo AS ME INNER JOIN
			ExternalCustomerCargo AS OTHER ON ME.CargoId = OTHER.CargoId LEFT JOIN
			StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId AND OTHER.StationCustomerCargoId = SCW.OtherExternalCustomerCargoId
		WHERE
			ME.NHMCode > 0 AND ME.StationCustomerId = @StationCustomerId AND SCW.Id IS NOT NULL
	)
	UPDATE StationCustomerWaybill 
		SET 
			OperatingDayId = UW.OperatingDayId,
			OtherRegionId = UW.RegionId,
			PrintCount = UW.PrintCount
		FROM 
			UpdatedableWaybill UW
		WHERE
			StationCustomerWaybill.Id = UW.Id

	INSERT INTO StationCustomerWaybill
	SELECT
		ME.StationCustomerId AS [StationCustomerId],
		ME.StationCustomerCargoId AS [StationCustomerCargoId],
		NULL AS [OtherStationCustomerCargoId],
		OTHER.StationCustomerCargoId AS [OtherExternalCustomerCargoId],
		OTHER.RegionId AS [OtherRegionId],
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
		CASE WHEN ME.QuantityIsBearer <> 0 THEN ME.Quantity ELSE 1 END AS [PrintCount],
		0 AS [SequenceNumber]
	FROM
		ModuleCustomerCargo AS ME INNER JOIN
		ExternalCustomerCargo AS OTHER ON ME.CargoId = OTHER.CargoId LEFT JOIN
		StationCustomerWaybill AS SCW ON ME.StationCustomerCargoId = SCW.StationCustomerCargoId AND OTHER.StationCustomerCargoId = SCW.OtherExternalCustomerCargoId
	WHERE
		ME.NHMCode > 0 AND
		ME.StationCustomerId = @StationCustomerId
		AND ME.StationCustomerId <> OTHER.StationCustomerId
		AND ME.IsSupply <> OTHER.IsSupply
		--AND ME.QuantityUnitId = OTHER.QuantityUnitId
		AND ((ME.CountryId = OTHER.CountryId) OR (ME.IsInternational <> 0 AND OTHER.IsInternational <> 0))
		AND (ME.FromYear IS NULL OR OTHER.UptoYear IS NULL OR ME.FromYear <= OTHER.UptoYear )
		AND (ME.UptoYear IS NULL OR OTHER.FromYear IS NULL OR ME.UptoYear >= OTHER.FromYear )
		AND SCW.Id IS NULL

END