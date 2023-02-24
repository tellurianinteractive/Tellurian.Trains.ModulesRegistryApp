CREATE TABLE [dbo].[TimetableStretchLayoutTrackStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableStretchId] INT NOT NULL,
    [LayoutTrackStretchId] INT NOT NULL,
    [LayoutTrackStretchPosition] TINYINT NOT NULL,
    CONSTRAINT [PK_TimetableStretchLayoutTrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableStretchLayoutTrackStretch_TimetableStretch] FOREIGN KEY ([TimetableStretchId]) REFERENCES [dbo].[TimetableStretch] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableStretch_LayoutTrackStretch] FOREIGN KEY ([LayoutTrackStretchId]) REFERENCES [dbo].[LayoutTrackStretch] ([Id]),

)
