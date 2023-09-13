CREATE TABLE [dbo].[ModuleOnTimetableTrackStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableTrackStretchId] INT NOT NULL,
    [LayoutModuleId] INT NOT NULL,
    [Position] TINYINT NOT NULL,
    CONSTRAINT [PK_ModuleOnTimetableTrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ModuleOnTimetableTrackStretch_Position] UNIQUE ([Position], [TimetableTrackStretchId]),
    CONSTRAINT [FK_ModuleOnTimetableTrackStretch_LayoutTrackStretch] FOREIGN KEY ([TimetableTrackStretchId]) REFERENCES [dbo].[TimetableTrackStretch] ([Id]),
    CONSTRAINT [FK_ModuleOnTimetableTrackStretch_LayoutModule] FOREIGN KEY ([LayoutModuleId]) REFERENCES [dbo].[LayoutModule] ([Id]),
)
