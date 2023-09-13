CREATE TABLE [dbo].[TimetableStationTrack]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [TimetableStationId] INT NOT NULL,
    [StationTrackId] INT NULL,
    [Usage] NVARCHAR(50) NULL,
    [IsScheduled] BIT NOT NULL DEFAULT(1),
    CONSTRAINT [PK_TimetableStationTrack] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableStationTrack_TimetableStation] FOREIGN KEY ([TimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableStationTrack_StationTrack] FOREIGN KEY ([StationTrackId]) REFERENCES [dbo].[StationTrack] ([Id]),
)