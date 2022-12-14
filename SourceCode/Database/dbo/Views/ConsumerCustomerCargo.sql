CREATE VIEW [dbo].[ConsumerCustomerCargo] AS
	SELECT * FROM ExternalCustomerCargo AS ECC WHERE ECC.IsSupply = 0
	UNION
	SELECT * FROM ModuleCustomerCargo AS MCC WHERE MCC.IsSupply = 0
	UNION
	SELECT * FROM ShadowYardCustomerCargo AS SYCC WHERE SYCC.IsSupply = 0