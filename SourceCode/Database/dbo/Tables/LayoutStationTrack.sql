CREATE TABLE [dbo].[LayoutStationTrack]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [LayoutStationId] INT NOT NULL,
    [StationTrackId] INT NOT NULL,
    [Usage] NVARCHAR(50) NULL,
    [IsScheduled] BIT NULL,
    CONSTRAINT [PK_LayoutStationTrack] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LayoutStationTrack_LayoutStation] FOREIGN KEY ([LayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutStationTrack_StationTrack] FOREIGN KEY ([StationTrackId]) REFERENCES [dbo].[StationTrack] ([Id]),
)