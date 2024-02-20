CREATE TABLE [dbo].[LayoutStation]
(
	[Id]						  INT NOT NULL PRIMARY KEY IDENTITY,
	[LayoutParticipantId]         INT NOT NULL,
	[StationId]					  INT NOT NULL,
	[Difficulty]                  INT NOT NULL DEFAULT (1),
	[OtherName]					  NVARCHAR(50) NULL,
	[OtherSignature]			  NVARCHAR(5) NULL,
	[OtherCountryId]			  INT NULL,
	[IsManned]					  BIT DEFAULT ((0)) NOT NULL,
	[PrintStationTimetable]		  BIT DEFAULT ((0)) NOT NULL,
	[PrintStoppingTrainsOnly]	  BIT DEFAULT ((0)) NOT NULL,
	[PrintBlockDestinations]	  BIT DEFAULT ((0)) NOT NULL,
	[PrintInstructions]			  BIT DEFAULT ((0)) NOT NULL,
	[HasCombinedIntructions]	  BIT DEFAULT ((0)) NOT NULL,
	[PartOfOtherLayoutStationId]  INT NULL,
	[StationInstructions]		  NVARCHAR(2000) NULL,
	[ShuntingInstructions]		  NVARCHAR(2000) NULL,
	CONSTRAINT [FK_LayoutStation_LayoutParticipant] FOREIGN KEY ([LayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_LayoutStation_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]),
	CONSTRAINT [FK_LayoutStation_Country] FOREIGN KEY ([OtherCountryId]) REFERENCES [dbo].[Country] ([Id]),
	CONSTRAINT [FK_LayoutStation_IsPartOf_LayoutStation] FOREIGN KEY ([PartOfOtherLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id])
)
GO
CREATE INDEX [IX_LayoutStation_LayoutParticipantId] ON [LayoutStation] ([LayoutParticipantId])
GO
CREATE INDEX [IX_LayoutStation_StationId] ON [LayoutStation] (StationId)
GO
