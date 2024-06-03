CREATE TABLE [dbo].[Vehicle]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [KeeperSignature] VARCHAR(10) NOT NULL, 
    [VehicleClass] VARCHAR(10) NOT NULL,
    [VehicleNumber] VARCHAR(20) NOT NULL,
    [InteroperabilityId] INT NOT NULL,
    [PrototypeManufacturerName] NVARCHAR(20) NULL,
    [ScaleId] INT NOT NULL,
    [ModelManufacturerName] NVARCHAR(20) NULL,
    [ModelImage] VARBINARY(MAX) NULL,
    [ModelNumber] NVARCHAR(10) NULL,
    [CouplingFeatureId] INT NULL,
    [WheelsFeatureId] INT NULL,
    [ThisEmbodiementFromYear] SMALLINT NULL,
    [ThisEmbodiementUptoYear] SMALLINT NULL,
    [IsWeathered] BIT NOT NULL DEFAULT((0)),
    [Theme] VARCHAR(10) NOT NULL,
    [OwningPersonId] INT NOT NULL,
    [InventoryNumber] INT NOT NULL,
    [TractionFeatureId] INT NULL,
    [DccAddress] SMALLINT NULL,
    [DecoderType] NVARCHAR(20) NULL,
    [HasSound] BIT NOT NULL DEFAULT((0)),
    [HasRemoteCouplings] BIT NOT NULL DEFAULT((0)),

    [Note] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_Vehicle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Vehicle_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id]),
    CONSTRAINT [FK_Vehicle_CouplingFeature] FOREIGN KEY ([CouplingFeatureId]) REFERENCES [dbo].[VehicleFeature] ([Id]),
    CONSTRAINT [FK_Vehicle_WheelsFeature] FOREIGN KEY ([WheelsFeatureId]) REFERENCES [dbo].[VehicleFeature] ([Id]),
    CONSTRAINT [FK_Vehicle_OwningPersonId] FOREIGN KEY ([OwningPersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [UX_Vehicle_InventoryNumber] UNIQUE (OwningPersonId, InventoryNumber)


)
