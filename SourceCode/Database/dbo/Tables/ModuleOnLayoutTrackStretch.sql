CREATE TABLE [dbo].[ModuleOnLayoutTrackStretch]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutTrackStretchId] INT NOT NULL,
    [LayoutModuleId] INT NOT NULL,
    [Position] TINYINT NOT NULL,
    CONSTRAINT [PK_ModuleOnLayoutTrackStretch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ModuleOnLayoutTrackStretch_Position] UNIQUE ([Position], [LayoutTrackStretchId]),
    CONSTRAINT [FK_ModuleOnLayoutTrackStretch_LayoutTrackStretch] FOREIGN KEY ([LayoutTrackStretchId]) REFERENCES [dbo].[LayoutTrackStretch] ([Id]),
    CONSTRAINT [FK_ModuleOnLayoutTrackStretch_LayoutModule] FOREIGN KEY ([LayoutModuleId]) REFERENCES [dbo].[LayoutModule] ([Id]),
)
