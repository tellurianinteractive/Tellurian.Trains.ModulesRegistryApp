CREATE VIEW [dbo].[ShadowYardConsumerWaybill] AS 
-- These view must match colums in same order :
-- ModuleSupplierWaybill
-- ModuleConsumerWaybill
-- ExternalSupplierWaybill
-- ExternalConsumerWaybill
-- ShadowYardSupplierWaybill
-- ShadowYardConsumerWaybill

SELECT DISTINCT
    'ShadowYardConsumer' AS [Source],
    SCW.Id,
    SCW.PrintCount,
    SCW.PrintPerOperatingDay,
    SCW.HasEmptyReturn,
    SENDER.StationId AS OriginStationId,
    SENDER.StationCustomerId AS OriginStationCustomerId,
    SENDER.StationName AS OriginStationName,
    SENDER.InternationalStationName AS OriginInternationalStationName,
    SENDER.CustomerName AS SenderName,
    SENDER.CustomerTrackOrArea AS SenderTrackOrArea,
    SENDER.CustomerTrackOrAreaColor AS SenderTrackOrAreaColor,
    SENDER.CargoTrackOrArea AS SenderCargoTrackOrArea,
    SENDER.CargoTrackOrAreaColor AS SenderCargoTrackOrAreaColor,
    SENDER.Languages AS OriginLanguages,
    SENDER.DomainSuffix AS OriginDomainSuffix,
    SENDER.BackColor AS OriginBackColor,
    SENDER.ForeColor AS OriginForeColor,
    SENDER.ReadyTime AS SenderReadyTime,
    SENDER.OperatingDayFlag AS SendingDayFlag,
    SENDER.IsModuleStation AS OriginIsModuleStation,
    SENDER.FromYear AS SenderFromYear,
    SENDER.UptoYear AS SenderUptoYear,
    CAST (1 AS BIT) OriginIsExternal,
    RECEIVER.StationId AS DestinationStationId,
    RECEIVER.StationCustomerId AS DestinationStationCustomerId,
    RECEIVER.StationName AS DestinationStationName,
    RECEIVER.InternationalStationName AS DestinationInternationalStationName,
    RECEIVER.CustomerName AS ReceiverName,
    RECEIVER.CustomerTrackOrArea AS ReceiverTrackOrArea,
    RECEIVER.CustomerTrackOrAreaColor AS ReceiverTrackOrAreaColor,
    RECEIVER.CargoTrackOrArea AS ReceiverCargoTrackOrArea,
    RECEIVER.CargoTrackOrAreaColor AS ReceiverCargoTrackOrAreaColor,
    RECEIVER.Languages AS DestinationLanguages,
    RECEIVER.DomainSuffix AS DestinationDomainSuffix,
    RECEIVER.BackColor AS DestinationBackColor,
    RECEIVER.ForeColor AS DestinationForeColor,
    RECEIVER.ReadyTime AS ReceiverReadyTime,
    RECEIVER.OperatingDayFlag AS ReceivingDayFlag,
    RECEIVER.IsModuleStation AS DestinationIsModuleStation,
    RECEIVER.SpecialCargoName,
    RECEIVER.SpecificWagonClass,
	RECEIVER.QuantityIsBearer,
    RECEIVER.QuantityUnitId,
    RECEIVER.Quantity,
    RECEIVER.QuantityUnitResourceCode AS QuanityUnitResourceName,
    RECEIVER.QuantityShortUnit,
    RECEIVER.PackagingUnit AS PackagingUnitResourceName,
    RECEIVER.PackagingPrepositionResourceCode,
    RECEIVER.FromYear AS ReceiverFromYear,
    RECEIVER.UptoYear AS ReceiverUptoYear,
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
    SupplierCustomerCargo AS SENDER ON SENDER.StationCustomerCargoId = SCW.StationCustomerCargoId INNER JOIN
    ShadowYardCustomerCargo AS RECEIVER ON RECEIVER.StationCustomerCargoId = SCW.StationCustomerCargoId	INNER JOIN
    Cargo AS C ON C.Id = SENDER.CargoId
WHERE
    SENDER.StationId <> RECEIVER.StationId AND
    SCW.OtherExternalCustomerCargoId IS NULL AND
    SCW.OtherStationCustomerCargoId IS NULL AND
    (C.FromYear IS NULL OR SENDER.UptoYear IS NULL OR ISNULL(C.FromYear,0) <= SENDER.UptoYear ) AND
    (C.UptoYear IS NULL OR SENDER.FromYear IS NULL OR ISNULL(C.UptoYear,9999) >= SENDER.FromYear ) 