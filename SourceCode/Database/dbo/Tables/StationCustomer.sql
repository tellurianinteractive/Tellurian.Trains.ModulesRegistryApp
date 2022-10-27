CREATE TABLE [dbo].[StationCustomer] (
    [Id]               INT          IDENTITY (1, 1) NOT NULL,
    [StationId]        INT          NOT NULL,
    [LayoutId]         INT          NULL,
    [CustomerName]     VARCHAR (50) NOT NULL,
    [Comment]          VARCHAR (50) NULL,
    [TrackOrArea]      NCHAR (10)   NULL,
    [TrackOrAreaColor] NCHAR (7)    NULL,
    [OpenedYear]       SMALLINT     NULL,
    [ClosedYear]       SMALLINT     NULL,
    CONSTRAINT [PK_StationCustomer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StationCustomer_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id])
);
GO
CREATE TRIGGER [DeleteStationCustomer] ON [StationCustomer] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [StationCustomerWaybill] WHERE [StationCustomerId] IN (SELECT [Id] FROM DELETED)
    DELETE FROM [StationCustomerCargo] WHERE [StationCustomerId] IN (SELECT [Id] FROM DELETED)
    DELETE FROM [StationCustomer] WHERE [Id] IN (SELECT [Id] FROM DELETED)
END


GO
CREATE NONCLUSTERED INDEX [IX_StationCustomer_StationId]
    ON [dbo].[StationCustomer]([StationId] ASC);

