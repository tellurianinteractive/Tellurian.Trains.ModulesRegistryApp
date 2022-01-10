CREATE TABLE [dbo].[LayoutStation]
(
	[Id]						INT NOT NULL PRIMARY KEY IDENTITY,
	[LayoutParticipantId]       INT NOT NULL,
	[StationId]					INT NOT NULL,
	[OtherName]					NVARCHAR(50) NULL,
	[OtherSignature]			NVARCHAR(5) NULL,
	[OtherCountryId]			INT NULL,
	[IsManned]					BIT CONSTRAINT [DF_LayoutStation_IsManned] DEFAULT ((0)) NOT NULL,
	[PrintStationTimetable]		BIT CONSTRAINT [DF_LayoutStation_[PrintStationTimetable] DEFAULT ((0)) NOT NULL,
	[PrintStoppingTrainsOnly]	BIT CONSTRAINT [DF_LayoutStation_ShowStoppingTrainsOnly] DEFAULT ((0)) NOT NULL,
	[PrintBlockDestinations]	BIT CONSTRAINT [DF_LayoutStation_PrintBlockDestinations] DEFAULT ((0)) NOT NULL,
	[PrintInstructions]			BIT CONSTRAINT [DF_LayoutStation_PrintInstructions] DEFAULT ((0)) NOT NULL,
	[HasCombinedIntructions]	BIT CONSTRAINT [DF_LayoutStation_HasCombinedIntructions] DEFAULT ((0)) NOT NULL,
	[StationInstructions]		NVARCHAR(2000) NULL,
	[ShuntingInstructions]		NVARCHAR(2000) NULL,
	CONSTRAINT [FK_LayoutStation_LayoutParticipant] FOREIGN KEY ([LayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]),
	CONSTRAINT [FK_LayoutStation_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]),
	CONSTRAINT [FK_LayoutStation_Country] FOREIGN KEY ([OtherCountryId]) REFERENCES [dbo].[Country] ([Id])
)
