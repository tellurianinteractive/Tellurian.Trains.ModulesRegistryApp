CREATE TABLE [dbo].[TimetableStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [Description] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_TimetableStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableStretch_Layout] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]) ON DELETE CASCADE,
)
GO
CREATE NONCLUSTERED INDEX [IX_TimetableStretch_LayoutId]
    ON [dbo].[TimetableStretch]([TimetableId] ASC);

