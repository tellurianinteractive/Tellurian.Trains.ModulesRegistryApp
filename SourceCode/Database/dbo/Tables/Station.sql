CREATE TABLE [dbo].[Station] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [FullName]         NVARCHAR (50) NOT NULL,
    [Signature]        NVARCHAR (5)  NOT NULL,
    [IsShadow]         BIT           CONSTRAINT [DF_Station_IsFiddleyard] DEFAULT ((0)) NOT NULL,
    [IsTerminus]       BIT           CONSTRAINT [DF_Station_IsJunction] DEFAULT ((0)) NOT NULL,
    [RegionId]         INT           NULL,
    [PdfInstructionId] INT           NULL,
    CONSTRAINT [PK_Station] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Station_Document] FOREIGN KEY ([PdfInstructionId]) REFERENCES [dbo].[Document] ([Id]),
    CONSTRAINT [FK_Station_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id])
);

