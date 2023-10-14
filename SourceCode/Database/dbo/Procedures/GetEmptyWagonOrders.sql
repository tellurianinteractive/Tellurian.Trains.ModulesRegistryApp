CREATE PROCEDURE [dbo].[GetEmptyWagonOrders]
	@StationId int,
	@CustomerId int = NULL
AS
	SELECT 
	MCC.StationId,
	MCC.StationName,
	MCC.StationSignature,
	MCC.CustomerId,
	MCC.CustomerName,
	MCC.Quantity AS NumberOfWagons,
	MCC.SpecificWagonClass,
	MCC.FromYear,
	MCC.UptoYear,
	MCC.CargoTrackOrArea,
	MCC.CustomerTrackOrArea,
	MCC.CargoTrackOrAreaColor,	
	MCC.CustomerTrackOrAreaColor,
	MCC.SpecificWagonClass,
	MCC.Languages,
	MCC.DomainSuffix,
	MCC.SpecialCargoName,
	C.*
FROM 
	ModuleCustomerCargo AS MCC INNER JOIN 
	Cargo AS C ON C.Id = MCC.CargoId
WHERE 
	MCC.IsSupply <> 0 AND
	MCC.StationId = @StationId AND
	(@CustomerId IS NULL OR MCC.CustomerId = @CustomerId)
RETURN 0
