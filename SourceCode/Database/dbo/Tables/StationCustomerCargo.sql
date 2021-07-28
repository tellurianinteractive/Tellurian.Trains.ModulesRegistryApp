CREATE TABLE [dbo].[StationCustomerCargo] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [CargoId]            INT           NOT NULL,
    [PackageUnitId]      INT           CONSTRAINT [DF_StationCustomerCargo_PackageUnitId] DEFAULT((0)) NOT NULL,
    [StationCustomerId]  INT           NOT NULL,
    [TrackOrArea]        NVARCHAR (10) NULL,
    [TrackOrAreaColor]   NCHAR (7)     NULL,
    [SpecificWagonClass] NVARCHAR(10)  NULL,
    [SpecialCargoName]   NVARCHAR (20) NULL,
    [DirectionId]        INT           NOT NULL,
    [OperatingDayId]     INT           CONSTRAINT [DF_StationCustomerCargo_OperatingDayId] DEFAULT ((0)) NOT NULL,
    [QuantityUnitId]     INT           NOT NULL,
    [Quantity]           INT           CONSTRAINT [DF_StationCustomerCargo_Quantity] DEFAULT ((0)) NOT NULL,
    [MaxTrainsetLength]  INT           NULL,
    [ReadyTimeId]        INT           NOT NULL,
    [FromYear]           SMALLINT      NULL,
    [UptoYear]           SMALLINT      NULL,
    CONSTRAINT [PK_CargoCustomer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CargoCustomer_Cargo] FOREIGN KEY ([CargoId]) REFERENCES [dbo].[Cargo] ([Id]),
    CONSTRAINT [FK_CargoCustomer_CargoDirection] FOREIGN KEY ([DirectionId]) REFERENCES [dbo].[CargoDirection] ([Id]),
    CONSTRAINT [FK_CargoCustomer_CargoReadyTime] FOREIGN KEY ([ReadyTimeId]) REFERENCES [dbo].[CargoReadyTime] ([Id]),
    CONSTRAINT [FK_CargoCustomer_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_CargoCustomer_StationCustomer] FOREIGN KEY ([StationCustomerId]) REFERENCES [dbo].[StationCustomer] ([Id]) ON DELETE CASCADE
);



