CREATE TABLE [dbo].[TrackStretch]
(
	[Id] INT NOT NULL IDENTITY (1, 1),
	[TimetableId] INT NOT NULL,
	[FromLayoutStationId] INT NOT NULL,
	[ToLayoutStationId] INT NOT NULL,
	[ParallelTracksCount] INT NOT NULL DEFAULT 1, /* Determines how many trains can be on the stretch at the same time */
	[DistanceMeters] FLOAT NULL, /* Actual meters on the layout */
	CONSTRAINT [PK_TimetableTrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_TimetableTrackStretch_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_TimetableTrackStretch_FromTimetableStation] FOREIGN KEY ([FromLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
	CONSTRAINT [FK_TimetableTrackStretch_ToTimetableStation] FOREIGN KEY ([ToLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
)
