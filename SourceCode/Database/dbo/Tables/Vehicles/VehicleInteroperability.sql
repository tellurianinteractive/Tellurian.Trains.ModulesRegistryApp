CREATE TABLE [dbo].[VehicleInteroperability]
(
	[Code] INT NOT NULL PRIMARY KEY,
	[Description] VARCHAR(100),
	[AppliesToTractionUnits] BIT NOT NULL DEFAULT((0)),
	[AppliesToFreightWagons] BIT NOT NULL DEFAULT((0)),
	[AppliesToPassengerCars] BIT NOT NULL DEFAULT((0)),
	[AppliesToBogieWagons] BIT NOT NULL DEFAULT((0)),
	[AppliesToCarCarryingWagons] BIT NOT NULL DEFAULT((0)),
	[IsForInternationalUse] BIT NOT NULL DEFAULT((0)),
	[IsCompliantWithInternationalStandard] BIT NOT NULL DEFAULT((0)),
	[InternationalUseRequiresSpecialAgreement] BIT NOT NULL DEFAULT((0)),
	[IsSpecialVehicle] BIT NOT NULL DEFAULT((0)),
	[WithAirCondition]  BIT NOT NULL DEFAULT((0)),

)
