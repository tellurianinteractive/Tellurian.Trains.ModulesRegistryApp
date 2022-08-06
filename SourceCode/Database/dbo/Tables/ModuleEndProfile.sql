CREATE TABLE [dbo].[ModuleEndProfile] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [ScaleId]       INT           NOT NULL,
    [Designation]   NVARCHAR (20) NOT NULL,
    [PdfDocumentId] INT           NULL,
    CONSTRAINT [PK_ModuleEndProfile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleEndProfile_Document] FOREIGN KEY ([PdfDocumentId]) REFERENCES [dbo].[Document] ([Id]),
    CONSTRAINT [FK_ModuleEndProfile_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id])
);



