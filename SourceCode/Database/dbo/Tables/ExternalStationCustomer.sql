CREATE TABLE [dbo].[ExternalStationCustomer] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [ExternalStationId] INT           NOT NULL,
    [CustomerName]      NVARCHAR (50) NOT NULL,
    [OpenedYear]        SMALLINT      NULL,
    [ClosedYear]        SMALLINT      NULL,
    CONSTRAINT [PK_ExternalStationCustomer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalStationCustomer_ExternalStation] FOREIGN KEY ([ExternalStationId]) REFERENCES [dbo].[ExternalStation] ([Id]) ON DELETE CASCADE
);

