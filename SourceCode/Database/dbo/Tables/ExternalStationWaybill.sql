CREATE TABLE [dbo].[ExternalCustomerWaybill]
(
    [Id]                        INT IDENTITY (1, 1) NOT NULL,
    [StationCustomerId]         INT NOT NULL,
    [StationCustomerCargoId]    INT NOT NULL,
    [OtherCustomerCargoId]      INT NOT NULL,
    [OtherRegionId]             INT NOT NULL,
    [OperatingDayId]            INT DEFAULT ((8)) NOT NULL,
    [IsManuallyCreated]         BIT DEFAULT ((0)) NOT NULL,
    [HasEmptyReturn]            BIT DEFAULT ((0)) NOT NULL,
    [HideLoadingTimes]          BIT DEFAULT ((0)) NOT NULL,
    [HideUnloadingTimes]        BIT DEFAULT ((0)) NOT NULL,
    [PrintPerOperatingDay]      BIT DEFAULT ((0)) NOT NULL,
    [PrintCount]                INT DEFAULT ((1)) NOT NULL,
    [SequenceNumber]            INT DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ExternalCustomerWaybill] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalCustomerWaybill_StationCustomerId] FOREIGN KEY ([StationCustomerId]) REFERENCES [dbo].[StationCustomer] ([Id]) ON DELETE CASCADE ,
    CONSTRAINT [FK_ExternalCustomerWaybill_StationCustomerCargoId] FOREIGN KEY ([StationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_ExternalCustomerWaybill_OtherCustomerCargoId] FOREIGN KEY ([OtherCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_ExternalCustomerWaybill_OtherRegionId] FOREIGN KEY ([OtherRegionId]) REFERENCES [dbo].[Region] ([Id]),
    CONSTRAINT [FK_ExternalCustomerWaybill_OperatingDayId] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
)
GO
CREATE NONCLUSTERED INDEX [IX_StationCustomerWaybill_StationCustomerId]
    ON [dbo].[ExternalCustomerWaybill] ([StationCustomerId] ASC, [StationCustomerCargoId] ASC)
    INCLUDE ([OtherCustomerCargoId], [OtherRegionId],[OperatingDayId])


