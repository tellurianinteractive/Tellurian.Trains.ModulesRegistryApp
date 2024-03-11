CREATE TABLE [dbo].[TimetableStretchTrackStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableStretchId] INT NOT NULL,
    [TimetableTrackStretchId] INT NOT NULL,
    [TrackStretchPosition] TINYINT NOT NULL,
    CONSTRAINT [PK_TimetableStretchTrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableStretchTrackStretch_TimetableStretch] FOREIGN KEY ([TimetableStretchId]) REFERENCES [dbo].[TimetableStretch] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableStretch_TrackStretch] FOREIGN KEY ([TimetableTrackStretchId]) REFERENCES [dbo].[TrackStretch] ([Id]),

)
