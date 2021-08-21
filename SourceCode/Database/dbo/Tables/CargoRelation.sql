-- Not implemented. Waybills are created from a view.

CREATE TABLE [dbo].[CargoRelation] (
    [Id]                             INT IDENTITY (1, 1) NOT NULL,
    [SupplierStationCustomerCargoId] INT NOT NULL,
    [ConsumerStationCustomerCargoId] INT NOT NULL,
    [DefaultWagonClassId]            INT NOT NULL,
    [OperatingDayId]                 INT NULL,
    [OperatorId]                     INT NULL,
    [Layout]                         INT NULL,
    CONSTRAINT [PK_CargoRelation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CargoRelation_CargoCustomer_Consumer] FOREIGN KEY ([ConsumerStationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_CargoRelation_CargoCustomer_Supplier] FOREIGN KEY ([SupplierStationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_CargoRelation_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_CargoRelation_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Operator] ([Id])
);

