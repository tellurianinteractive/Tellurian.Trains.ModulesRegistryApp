CREATE TABLE [dbo].[LayoutVehiclesImported]
(
	[DccAddress] SMALLINT NOT NULL,
	[OperatorSignature] NVARCHAR(10) NOT NULL,
	[VehicleClass] NVARCHAR(10) NOT NULL,
	[VehicleNumber] NVARCHAR(16) NOT NULL,
	[VehicleProviderName] NVARCHAR(50) NULL,
	[IsTractionUnit] BIT NOT NULL DEFAULT ((0)),
	[ThrottleIdentity] NVARCHAR(50) NULL,
	[LayoutId] INT NOT NULL

	CONSTRAINT [PK_LayoutVehicleImported] PRIMARY KEY CLUSTERED ([DccAddress] ASC),
    CONSTRAINT [FK_LayoutVehicleImported_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),

)
