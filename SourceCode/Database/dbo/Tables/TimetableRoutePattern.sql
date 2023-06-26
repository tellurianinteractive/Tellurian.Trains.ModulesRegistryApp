CREATE TABLE [dbo].[TimetableRoutePattern]
(
	[Id] INT NOT NULL IDENTITY (1, 1),
	[TimetableId] INT NOT NULL,
	[Description] NVARCHAR(10) NOT NULL,
	[TrainPreparationMinutes] TINYINT DEFAULT 0, /* Minutes before first departure when driver must be present */
	[TrainFinishingMinutes] TINYINT DEFAULT 0, /* Minutes after last arrival when driver must be present */
	CONSTRAINT [PK_TimetableRoutePattern] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_TimetableRoutePattern_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),

)
