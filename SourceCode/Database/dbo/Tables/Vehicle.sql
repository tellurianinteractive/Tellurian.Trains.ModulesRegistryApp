CREATE TABLE [dbo].[Vehicle]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [KeeperOperatorId] INT NOT NULL, 
    [Class] VARCHAR(10) NOT NULL,
    [Number] VARCHAR(10) NOT NULL,
    [InteroperabilityId] INT NOT NULL,
    [PrototypeManufacturerName] NVARCHAR(20) NULL,
    [ScaleId] INT NOT NULL,
    [ModelManufacturerName] NVARCHAR(20) NULL,
    [ModelImage] VARBINARY(MAX) NULL,
    [CouplingFeatureId] INT NULL,
    [WheelsFeatureId] INT NULL,
    [ThisEmpodiementFromYear] SMALLINT NULL,
    [ThisEmpodiementUptoYear] SMALLINT NULL,
    [IsWeathered] BIT NOT NULL DEFAULT((0)),
    [Theme] VARCHAR(10) NOT NULL,
    [OwningPersonId] INT NOT NULL,
    [InventoryNumber] INT NOT NULL,
    [TractionTypeId] INT NULL,
    [DccAddress] SMALLINT NULL,
    [DecoderType] NVARCHAR(20) NULL,
    [HasSound] BIT NOT NULL DEFAULT((0)),
    [HasRemoteCouplings] BIT NOT NULL DEFAULT((0)),

    CONSTRAINT [PK_Vehicle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Vehicle_KeeperOperator] FOREIGN KEY ([KeeperOperatorId]) REFERENCES [dbo].[VehicleOperator] ([Id]),
    CONSTRAINT [FK_Vehicle_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id]),
    CONSTRAINT [FK_Vehicle_CouplingFeature] FOREIGN KEY ([CouplingFeatureId]) REFERENCES [dbo].[VehicleFeature] ([Id]),
    CONSTRAINT [FK_Vehicle_WheelsFeature] FOREIGN KEY ([WheelsFeatureId]) REFERENCES [dbo].[VehicleFeature] ([Id]),
    CONSTRAINT [FK_Vehicle_OwningPersonId] FOREIGN KEY ([OwningPersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [UX_Vehicle_InventoryNumber] UNIQUE (OwningPersonId, InventoryNumber)


)
