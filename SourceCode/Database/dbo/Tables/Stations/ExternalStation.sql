﻿CREATE TABLE [dbo].[ExternalStation] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [RegionId]          INT           NOT NULL,
    [FullName]          NVARCHAR (50) NOT NULL,
    [Signature]         NVARCHAR (6)  NOT NULL,
    [Note]              NVARCHAR (20) NULL,
    [OpenedYear]        SMALLINT      NULL,
    [ClosedYear]        SMALLINT      NULL,
    [InternationalName] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_ExternalStation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ExternalStation_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id])
);

GO
CREATE TRIGGER [dbo].[DeleteExternalStation] ON [ExternalStation] INSTEAD OF DELETE
AS
BEGIN
    DELETE FROM [ExternalStationCustomer] WHERE ExternalStationId IN (SELECT Id FROM DELETED)
    DELETE FROM [ExternalStation] WHERE Id IN (SELECT Id FROM DELETED)
END

GO
CREATE NONCLUSTERED INDEX [IX_ExternalStation_RegionId]
    ON [dbo].[ExternalStation]([RegionId] ASC);




