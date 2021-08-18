CREATE TABLE [dbo].[LayoutStation]
(
	[Id]						INT NOT NULL PRIMARY KEY IDENTITY,
	[LayoutId]					INT NOT NULL,
	[StationId]					INT NOT NULL,
	[OtherName]					NVARCHAR(50) NULL,
	[OtherSignature]			NVARCHAR(5) NULL,
	[OtherCountryId]			INT NULL,
	[PhoneNumbers]				NVARCHAR(10) NULL,
	[PhoneNote]					NVARCHAR(20) NULL,
	[IsManned]					BIT CONSTRAINT [DF_LayoutStation_IsManned] DEFAULT ((0)) NOT NULL,
	[IsCargoHub]				BIT CONSTRAINT [DF_LayoutStation_IsCargoHub] DEFAULT ((0)) NOT NULL,
	[PrintStationTimetable]		BIT CONSTRAINT [DF_LayoutStation_[PrintStationTimetable] DEFAULT ((0)) NOT NULL,
	[ShowStoppingTrainsOnly]	BIT CONSTRAINT [DF_LayoutStation_ShowStoppingTrainsOnly] DEFAULT ((0)) NOT NULL,
	[PrintBlockDestinations]	BIT CONSTRAINT [DF_LayoutStation_PrintBlockDestinations] DEFAULT ((0)) NOT NULL,
	[ReverseBlockDestinations]	BIT CONSTRAINT [DF_LayoutStation_ReverseBlockDestinations] DEFAULT ((0)) NOT NULL,
	[PrintInstructions]			BIT CONSTRAINT [DF_LayoutStation_PrintInstructions] DEFAULT ((0)) NOT NULL,
	[HasCombinedIntructions]	BIT CONSTRAINT [DF_LayoutStation_HasCombinedIntructions] DEFAULT ((0)) NOT NULL,
	[StationInstructions]		NVARCHAR(2000) NULL,
	[ShuntingInstructions]		NVARCHAR(2000) NULL,
	CONSTRAINT [FK_LayoutStation_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_LayoutStation_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]),
	CONSTRAINT [FK_LayoutStation_Country] FOREIGN KEY ([OtherCountryId]) REFERENCES [dbo].[Country] ([Id])
)
