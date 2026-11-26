CREATE TABLE [dbo].[Feature] (
    [Id]             INT           NOT NULL,
    [Category]       VARCHAR (10)  NOT NULL,
    [OnlyScaleId]    INT           NULL,
    [Description]    NVARCHAR (20) NULL,
    [IsResourceCode] BIT           NOT NULL DEFAULT (0),
    CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Feature_Scale] FOREIGN KEY ([OnlyScaleId]) REFERENCES [dbo].[Scale] ([Id]),
    CONSTRAINT [UX_Feature] UNIQUE ([Category], [Description], [OnlyScaleId])
);