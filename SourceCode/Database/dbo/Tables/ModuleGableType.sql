CREATE TABLE [dbo].[ModuleGableType] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [ScaleId]       INT           NOT NULL,
    [Designation]   NVARCHAR (10) NOT NULL,
    [PdfDocumentId] INT           NULL,
    CONSTRAINT [PK_ModuleGableType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleGableType_Document] FOREIGN KEY ([PdfDocumentId]) REFERENCES [dbo].[Document] ([Id]),
    CONSTRAINT [FK_ModuleGableType_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id])
);

