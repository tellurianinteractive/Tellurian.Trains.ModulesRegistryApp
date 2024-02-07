CREATE TABLE [dbo].[StationCustomerWaybill]
(
    [Id]                            INT IDENTITY (1, 1) NOT NULL,
    [StationCustomerId]             INT NOT NULL,
    [StationCustomerCargoId]        INT NOT NULL,
    [OtherStationCustomerCargoId]   INT NULL,
    [OtherExternalCustomerCargoId]  INT NULL,
    [OtherRegionId]                 INT NULL,
    [OperatingDayId]                INT DEFAULT ((8)) NOT NULL,
    [IsManuallyCreated]             BIT DEFAULT ((0)) NOT NULL,
    [HasEmptyReturn]                BIT DEFAULT ((0)) NOT NULL,
    [HideLoadingTimes]              BIT DEFAULT ((0)) NOT NULL,
    [HideUnloadingTimes]            BIT DEFAULT ((0)) NOT NULL,
    [PrintPerOperatingDay]          BIT DEFAULT ((0)) NOT NULL,
    [PrintCount]                    INT DEFAULT ((1)) NOT NULL,
    [SequenceNumber]                INT DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_StationCustomerWaybill] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StationCustomerWaybill_StationCustomerId] FOREIGN KEY ([StationCustomerId]) REFERENCES [dbo].[StationCustomer] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_StationCustomerCargoId] FOREIGN KEY ([StationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OtherCustomerCargoId] FOREIGN KEY ([OtherStationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_ExternalCustomerCargoId] FOREIGN KEY ([OtherExternalCustomerCargoId]) REFERENCES [dbo].[ExternalStationCustomerCargo] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OtherRegionId] FOREIGN KEY ([OtherRegionId]) REFERENCES [dbo].[Region] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OperatingDayId] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
)
GO
CREATE INDEX [IX_StationCustomerWaybill_OtherExternalCustomerCargoId] ON [StationCustomerWaybill] ([OtherExternalCustomerCargoId])
GO
CREATE NONCLUSTERED INDEX [IX_StationCustomerWaybill_StationCustomerId]
    ON [dbo].[StationCustomerWaybill] ([StationCustomerId] ASC, [StationCustomerCargoId] ASC)
    INCLUDE ([OtherStationCustomerCargoId], [OtherExternalCustomerCargoId], [OtherRegionId],[OperatingDayId])


