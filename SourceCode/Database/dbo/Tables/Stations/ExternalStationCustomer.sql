CREATE TABLE [dbo].[ExternalStationCustomer] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [ExternalStationId] INT           NOT NULL,
    [CustomerName]      NVARCHAR (50) NOT NULL,
    [OpenedYear]        SMALLINT      NULL,
    [ClosedYear]        SMALLINT      NULL,
    CONSTRAINT [PK_ExternalStationCustomer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalStationCustomer_ExternalStation] FOREIGN KEY ([ExternalStationId]) REFERENCES [dbo].[ExternalStation] ([Id]) 
);

GO
CREATE TRIGGER [dbo].[DeleteExternalStationCustomer] ON [ExternalStationCustomer] INSTEAD OF DELETE
AS
BEGIN
    DELETE FROM [ExternalStationCustomerCargo] WHERE ExternalStationCustomerId IN (SELECT [Id] FROM DELETED)
    DELETE FROM [ExternalStationCustomer] WHERE Id IN (SELECT [Id] FROM DELETED)
END




GO
CREATE NONCLUSTERED INDEX [IX_ExternalStationCustomer_ExternalStationId]
    ON [dbo].[ExternalStationCustomer]([ExternalStationId] ASC);

