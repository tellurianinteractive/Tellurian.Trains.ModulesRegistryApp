CREATE TABLE [dbo].[TimetableStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutId] INT NOT NULL,
    [Description] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TimetableStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableStretch_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]) ON DELETE CASCADE,

)
