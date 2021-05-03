CREATE TABLE [dbo].[StationCustomer] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [StationId]        INT           NOT NULL,
    [LayoutId]         INT           NULL,
    [CustomerName]     VARCHAR (50)  NOT NULL,
    [Comment]          VARCHAR (50)  NULL,
    [TrackOrArea]      NVARCHAR (50) NULL,
    [TrackOrAreaColor] NCHAR (7)     NULL,
    [OpenedYear]       SMALLINT      NULL,
    [ClosedYear]       SMALLINT      NULL,
    CONSTRAINT [PK_StationCustomer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StationCustomer_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]) ON DELETE CASCADE
);

