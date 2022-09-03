CREATE PROCEDURE [dbo].[GetStationCustomerWaybills]
	@StationId INT,
	@StationCustomerId INT = NULL,
	@Sending BIT = 1,
	@Receiving BIT = 1
AS
BEGIN
	IF @Sending <> 0 AND @Receiving <> 0
		BEGIN
			SELECT * FROM ModuleSupplierWaybill AS MSW 
				WHERE MSW.DestinationStationId = @StationId AND (@StationCustomerId IS NULL OR MSW.DestinationStationCustomerId = @StationCustomerId)
			UNION
			SELECT * FROM ModuleConsumerWaybill AS MCW
				WHERE MCW.OriginStationId = @StationId AND (@StationCustomerId IS NULL OR MCW.OriginStationCustomerId = @StationCustomerId)
		END
	ELSE IF @Receiving <> 0 
		BEGIN
			SELECT * FROM ModuleSupplierWaybill AS MSW 
				WHERE MSW.DestinationStationId = @StationId AND (@StationCustomerId IS NULL OR MSW.DestinationStationCustomerId = @StationCustomerId)
		END
	ELSE IF @Sending <> 0
		BEGIN
			SELECT * FROM ModuleConsumerWaybill AS MCW
				WHERE MCW.OriginStationId = @StationId AND (@StationCustomerId IS NULL OR MCW.OriginStationCustomerId = @StationCustomerId)
		END
	RETURN 0
END
