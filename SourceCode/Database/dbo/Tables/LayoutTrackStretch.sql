CREATE TABLE [dbo].[LayoutTrackStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutId] INT NOT NULL,
    [FromLayoutStationId] INT NOT NULL,
	[FromModuleExitId] INT NULL,
    [ToLayoutStationId] INT NOT NULL,
    [ToModuleExitId] INT NULL,
    [ParallelTracksCount] INT NOT NULL DEFAULT 1,
    [DistanceMeters] FLOAT NULL,
    CONSTRAINT [PK_TrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TrackStretch_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackStretch_FromLayoutStation] FOREIGN KEY ([FromLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
 	CONSTRAINT [FK_TrackStretch_FromStationExit] FOREIGN KEY ([FromModuleExitId]) REFERENCES [dbo].[ModuleExit] ([Id]),
    CONSTRAINT [FK_TrackStretch_ToLayoutStation] FOREIGN KEY ([ToLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
 	CONSTRAINT [FK_TrackStretch_ToStationExit] FOREIGN KEY ([FromModuleExitId]) REFERENCES [dbo].[ModuleExit] ([Id]),


)
