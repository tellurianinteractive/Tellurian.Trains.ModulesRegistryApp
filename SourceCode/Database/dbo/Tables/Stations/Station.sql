CREATE TABLE [dbo].[Station] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [FullName]           NVARCHAR (50) NOT NULL,
    [Signature]          NVARCHAR (5)  NOT NULL,
    [IsShadow]           BIT           NOT NULL,
    [IsTerminus]         BIT           NOT NULL,
    [IsHarbour]          BIT           NOT NULL DEFAULT(0),
    [IsJunction]         BIT           NOT NULL DEFAULT(0),
    [IsKeyRequired]      BIT           NOT NULL DEFAULT(0),
    [HasCargoCustomers]  BIT           NOT NULL DEFAULT(1), 
    [RegionId]           INT           NULL,
    [PdfInstructionId]   INT           NULL,
    [PrimaryModuleId]    INT           NULL, 
    [BoosterTypes]       NVARCHAR(50) NULL, 
    [TypeOfSwitchGearId] INT NULL, 
    [OperationInstructionsMarkdown] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_Station] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Station_Document] FOREIGN KEY ([PdfInstructionId]) REFERENCES [dbo].[Document] ([Id]),
    CONSTRAINT [FK_Station_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id]),
    CONSTRAINT [FK_Station_Module] FOREIGN KEY (PrimaryModuleId) REFERENCES [dbo].[Module] ([Id])
);
GO
CREATE TRIGGER [DeleteStation] ON [Station] INSTEAD OF DELETE 
AS
BEGIN
    UPDATE [Module] SET [StationId] = NULL WHERE ISNULL([StationId],0) IN (SELECT [Id] FROM DELETED)
    DELETE FROM [StationCustomer] WHERE [StationId] IN (SELECT [Id] FROM DELETED)
    DELETE FROM [LayoutModule] WHERE ISNULL([LayoutStationId],0) IN (SELECT [Id] FROM [LayoutStation] WHERE [StationId] IN (SELECT [Id] FROM DELETED))
    DELETE FROM [LayoutStation] WHERE [StationId] IN (SELECT [Id] FROM DELETED)
    DELETE FROM [Station] WHERE [Id] IN (SELECT [Id] FROM DELETED)
END



GO
CREATE NONCLUSTERED INDEX [IX_Station_RegionId]
    ON [dbo].[Station]([RegionId] ASC);

