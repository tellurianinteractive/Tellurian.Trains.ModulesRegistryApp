-- Migration step 5.3: drops the procedure and views that made a station's waybills until release 1.17.2, when the app started
-- making them in code (StationWaybillGenerator). Run once in SQL Server Management Studio, if publishing the SQL project
-- doesn't drop them. The base views ModuleCustomerCargo, ExternalCustomerCargo and ShadowYardCustomerCargo are kept:
-- the waybill generation procedures and GetEmptyWagonOrders use them.

DROP PROCEDURE IF EXISTS [dbo].[GetStationCustomerWaybills];
DROP VIEW IF EXISTS [dbo].[ModuleSupplierWaybill];
DROP VIEW IF EXISTS [dbo].[ModuleConsumerWaybill];
DROP VIEW IF EXISTS [dbo].[ExternalSupplierWaybill];
DROP VIEW IF EXISTS [dbo].[ExternalConsumerWaybill];
DROP VIEW IF EXISTS [dbo].[ShadowYardSupplierWaybill];
DROP VIEW IF EXISTS [dbo].[ShadowYardConsumerWaybill];
DROP VIEW IF EXISTS [dbo].[SupplierCustomerCargo];
DROP VIEW IF EXISTS [dbo].[ConsumerCustomerCargo];