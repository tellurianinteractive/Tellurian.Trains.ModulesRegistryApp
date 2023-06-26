CREATE TABLE [dbo].[TimetableStation]
(
	[Id]							INT IDENTITY (1, 1) NOT NULL,
	[TimetableId]					INT NOT NULL,
	[Name]							NVARCHAR (50) NOT NULL,
	[Signature]						NVARCHAR (6) NOT NULL,
	[Difficulty]					INT NOT NULL DEFAULT (1),
	[IsManned]						BIT DEFAULT ((0)) NOT NULL,
	[PrintStationTimetable]			BIT DEFAULT ((0)) NOT NULL,
	[PrintStoppingTrainsOnly]		BIT DEFAULT ((0)) NOT NULL,
	[PrintBlockDestinations]		BIT DEFAULT ((0)) NOT NULL,
	[PrintInstructions]				BIT DEFAULT ((0)) NOT NULL,
	[HasCombinedIntructions]		BIT DEFAULT ((0)) NOT NULL,
	[PartOfOtherTimetableStationId] INT NULL,
	[StationInstructions]			NVARCHAR(2000) NULL,
	[ShuntingInstructions]			NVARCHAR(2000) NULL,

	CONSTRAINT [PK_TimetableStation] PRIMARY KEY CLUSTERED ([Id] ASC),      
	CONSTRAINT [FK_TimetableStation_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
	CONSTRAINT [FK_TimetableStation_PartOfOtherTimetableStation] FOREIGN KEY ([PartOfOtherTimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]),

)
