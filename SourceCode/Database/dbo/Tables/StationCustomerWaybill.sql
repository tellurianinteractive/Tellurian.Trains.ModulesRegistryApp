CREATE TABLE [dbo].[StationCustomerWaybill]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [StationCustomerId] INT NOT NULL,
    [StationCustomerCargoId] INT NOT NULL,
    [OtherCustomerCargoId] INT NULL,
    [OtherRegionId] INT NULL,
    [OperatingDayId] INT CONSTRAINT [DF_StationCustomerWaybill_OperationDay] DEFAULT ((8)) NOT NULL,
    [HasEmptyReturn] BIT CONSTRAINT [DF_StationCustomerWaybil_HasEmptyReturn] DEFAULT ((0)) NOT NULL,
    [IsExpressCargo] BIT CONSTRAINT [DF_StationCustomerWaybil_IsExpressCargo] DEFAULT ((0)) NOT NULL,
    [IsCoolingTransport] BIT CONSTRAINT [DF_StationCustomerWaybil_IsCoolingTransport] DEFAULT ((0)) NOT NULL,
    [HideLoadingTimes] BIT CONSTRAINT [DF_StationCustomerWaybil_HideLoadingTimes] DEFAULT ((0)) NOT NULL,
    [HideUnloadingTimes] BIT CONSTRAINT [DF_StationCustomerWaybil_HideUnloadingTimes] DEFAULT ((0)) NOT NULL,
    [SequenceNumber] INT CONSTRAINT [DF_StationCustomerWaybil_SequenceNumber] DEFAULT ((1)) NOT NULL,
    [PrintPerOperatingDay] BIT CONSTRAINT [DF_StationCustomerWaybil_PrintPerOperatingDay] DEFAULT ((0)) NOT NULL,
    [PrintCount] INT CONSTRAINT [DF_StationCustomerWaybil_PrintCount] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_StationCustomerWaybill] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StationCustomerWaybill_StationCustomerId] FOREIGN KEY ([StationCustomerId]) REFERENCES [dbo].[StationCustomer] ([Id]) ,
    CONSTRAINT [FK_StationCustomerWaybill_StationCustomerCargoId] FOREIGN KEY ([StationCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OtherCustomerCargoId] FOREIGN KEY ([OtherCustomerCargoId]) REFERENCES [dbo].[StationCustomerCargo] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OtherRegionId] FOREIGN KEY ([OtherRegionId]) REFERENCES [dbo].[Region] ([Id]),
    CONSTRAINT [FK_StationCustomerWaybill_OperatingDayId] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
)
GO
CREATE NONCLUSTERED INDEX [IX_StationCustomerWaybill_StationCustomerId]
    ON [dbo].[StationCustomerWaybill] ([StationCustomerId] ASC, [StationCustomerCargoId] ASC)
    INCLUDE ([OtherCustomerCargoId], [OtherRegionId],[OperatingDayId])


