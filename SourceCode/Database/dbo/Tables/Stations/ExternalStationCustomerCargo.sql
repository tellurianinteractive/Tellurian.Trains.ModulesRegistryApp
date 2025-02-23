﻿CREATE TABLE [dbo].[ExternalStationCustomerCargo] (
    [Id]                        INT           IDENTITY (1, 1) NOT NULL,
    [CargoId]                   INT           NOT NULL,
    [PackageUnitId]             INT           CONSTRAINT [DF_ExternalStationCustomerCargo_PackageUnitId] DEFAULT((0)) NOT NULL,
    [ExternalStationCustomerId] INT           NOT NULL,
    [SpecificWagonClass]        NVARCHAR (10) NULL,
    [SpecialCargoName]          NVARCHAR (20) NULL,
    [DirectionId]               INT           NOT NULL,
    [OperatingDayId]            INT           CONSTRAINT [DF_ExternalStationCustomerCargo_OperatingDayId] DEFAULT ((0)) NOT NULL,
    [QuantityUnitId]            INT           NOT NULL,
    [Quantity]                  INT           NOT NULL,
    [FromYear]                  SMALLINT      NULL,
    [UptoYear]                  SMALLINT      NULL,
    CONSTRAINT [PK_ExternalStationCustomerCargo] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalStationCustomerCargo_Cargo] FOREIGN KEY ([CargoId]) REFERENCES [dbo].[Cargo] ([Id]),
    CONSTRAINT [FK_ExternalStationCustomerCargo_CargoDirection] FOREIGN KEY ([DirectionId]) REFERENCES [dbo].[CargoDirection] ([Id]),
    CONSTRAINT [FK_ExternalStationCustomerCargo_CargoUnit] FOREIGN KEY ([QuantityUnitId]) REFERENCES [dbo].[CargoUnit] ([Id]),
    CONSTRAINT [FK_ExternalStationCustomerCargo_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_ExternalStationCustomerCargo_StationCustomer] FOREIGN KEY ([ExternalStationCustomerId]) REFERENCES [dbo].[ExternalStationCustomer] ([Id])
);

GO
CREATE TRIGGER [DeleteExternalStationCustomerCargo] ON [ExternalStationCustomerCargo] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [StationCustomerWaybill] WHERE ISNULL([OtherExternalCustomerCargoId],0) IN (SELECT [Id] FROM DELETED)
    DELETE FROM [ExternalStationCustomerCargo] WHERE [Id] IN (SELECT [Id] FROM DELETED)
END

GO
CREATE NONCLUSTERED INDEX [IX_ExternalStationCustomerCargo_StationCustomerId]
    ON [dbo].[ExternalStationCustomerCargo] ([ExternalStationCustomerId] ASC)
    INCLUDE ([CargoId], [PackageUnitId], [DirectionId] ,[OperatingDayId], [QuantityUnitId])

