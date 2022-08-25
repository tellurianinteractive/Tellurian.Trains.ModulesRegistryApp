CREATE VIEW [dbo].[ExternalSupplierWaybill] AS 
-- This view is waybills from suppliers at external stations.
-- This view must match colums in same order as in views:
-- ModuleSupplierWaybill
-- ModuleConsumerWaybill
-- ExternalSupplierWaybill (this)
-- ExternalConsumerWaybill
SELECT
	SENDER.StationName AS OriginStationName,
	SENDER.CustomerName AS SenderName,
	SENDER.TrackOrArea AS SenderTrackOrArea,
	SENDER.TrackOrAreaColor AS SenderTrackOrAreaColor,
	SENDER.Languages AS OriginLanguages,
	SENDER.DomainSuffix AS OriginDomainSuffix,
	SENDER.BackColor AS OriginBackColor,
	SENDER.ForeColor AS OriginForeColor,
	SENDER.ReadyTime AS SenderReadyTime,
	SENDER.OperatingDayFlag AS SendingDayFlag,
	SENDER.IsModuleStation AS OriginIsModuleStation,
	SENDER.SpecialCargoName,
	SENDER.Quantity,
	SENDER.QuantityUnitName AS QuanityUnitResourceName,
	SENDER.PackagingUnit AS PackagingUnitResourceName,
	CAST (1 AS BIT) OriginIsExternal,
	RECEIVER.StationName AS DestinationStationName,
	RECEIVER.CustomerName AS ReceiverName,
	RECEIVER.TrackOrArea AS ReceiverTrackOrArea,
	RECEIVER.TrackOrAreaColor AS ReceiverTrackOrAreaColor,
	RECEIVER.Languages AS DestinationLanguages,
	RECEIVER.DomainSuffix AS DestinationDomainSuffix,
	RECEIVER.BackColor AS DestinationBackColor,
	RECEIVER.ForeColor AS DestinationForeColor,
	RECEIVER.ReadyTime AS ReceiverReadyTime,
	RECEIVER.OperatingDayFlag AS ReceivingDayFlag,
	RECEIVER.IsModuleStation AS DestinationIsModuleStation,
	CAST (1 AS BIT) DestinationIsExternal,
	C.DefaultClasses,
	C.NHMCode,
	C.DA,
	C.DE,
	C.EN,
	C.FR,
	C.IT,
	C.NB,
	C.NL,
	C.PL,
	C.SV
FROM
	StationCustomerWaybill AS SCW INNER JOIN
	SupplierCustomerCargo RECEIVER ON SCW.StationCustomerCargoId = RECEIVER.StationCustomerCargoId INNER JOIN
	ConsumerCustomerCargo SENDER ON RECEIVER.IsModuleStation = 0 AND SCW.OtherExternalCustomerCargoId = SENDER.StationCustomerCargoId INNER JOIN
	Cargo AS C ON C.Id = SENDER.CargoId
WHERE
	(C.FromYear IS NULL OR SENDER.UptoYear IS NULL OR C.FromYear <= SENDER.UptoYear ) AND
	(C.UptoYear IS NULL OR SENDER.FromYear IS NULL OR C.UptoYear >= SENDER.FromYear )
