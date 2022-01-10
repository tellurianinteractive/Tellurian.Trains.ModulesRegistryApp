CREATE TABLE [dbo].[LayoutLine]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[LayoutId] INT NOT NULL,
	[FromLayoutStationId] INT NOT NULL,
	[FromStationExitId] INT NULL,
	[ToLayoutStationId] INT NOT NULL,
	[ToStationExitId] INT NULL,
	[TracksCount] SMALLINT NOT NULL,
	[DistanceMeters] REAL NULL,
	[MaxSpeed] SMALLINT NULL,
	CONSTRAINT [FK_LayoutLine_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),
	CONSTRAINT [FK_LayoutLine_FromLayoutStation] FOREIGN KEY ([FromLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
	CONSTRAINT [FK_LayoutLine_FromStationExit] FOREIGN KEY ([FromStationExitId]) REFERENCES [dbo].[ModuleExit] ([Id]),
	CONSTRAINT [FK_LayoutLine_ToLayoutStation] FOREIGN KEY ([ToLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
	CONSTRAINT [FK_LayoutLine_ToLayoutStationExit] FOREIGN KEY ([ToStationExitId]) REFERENCES [dbo].[ModuleExit] ([Id]),
)
