CREATE TABLE [dbo].[Station] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [FullName]         NVARCHAR (50) NOT NULL,
    [Signature]        NVARCHAR (5)  NOT NULL,
    [IsShadow]         BIT           NOT NULL,
    [IsTerminus]       BIT           NOT NULL,
    [RegionId]         INT           NULL,
    [PdfInstructionId] INT           NULL,
    CONSTRAINT [PK_Station] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Station_Document] FOREIGN KEY ([PdfInstructionId]) REFERENCES [dbo].[Document] ([Id]),
    CONSTRAINT [FK_Station_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id])
);
GO
CREATE TRIGGER [DeleteStation] ON [Station] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [StationCustomer] WHERE [StationId] IN (SELECT [Id] FROM DELETED)
END



GO
CREATE NONCLUSTERED INDEX [IX_Station_RegionId]
    ON [dbo].[Station]([RegionId] ASC);

