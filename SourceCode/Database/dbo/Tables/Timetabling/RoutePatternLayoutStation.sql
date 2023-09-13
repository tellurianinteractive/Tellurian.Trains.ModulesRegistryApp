CREATE TABLE [dbo].[TimetableRoutePatternTimetableStation]
(
	[Id] INT NOT NULL IDENTITY (1, 1),
	[TimetableRoutePatternId] INT NOT NULL,
	[TimetableStationId] INT NOT NULL,
	[OffsetMinutesFromFirstStation] SMALLINT DEFAULT 0, /* First station must have zero */
	[StopMinutes] TINYINT NOT NULL DEFAULT 0, /* Zero means no stop */
	CONSTRAINT [PK_TimetabletRoutePatternTimetableStation] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_TimetableRoutePatternTimetableStation_TimetableRoutePattern] FOREIGN KEY ([TimetableRoutePatternId]) REFERENCES [dbo].[TimetableRoutePattern] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_TimetableRoutePatternTimetableStation_TimetableStation] FOREIGN KEY ([TimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]),
)
