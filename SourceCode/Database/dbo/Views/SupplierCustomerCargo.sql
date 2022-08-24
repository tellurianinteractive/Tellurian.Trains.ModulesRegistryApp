CREATE VIEW [dbo].[SupplierCustomerCargo] AS 
	SELECT * FROM ExternalCustomerCargo AS ECC WHERE ECC.IsSupply <> 0
	UNION
	SELECT * FROM ModuleCustomerCargo AS MCC WHERE MCC.IsSupply <> 0