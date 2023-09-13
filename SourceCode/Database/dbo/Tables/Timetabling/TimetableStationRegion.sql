CREATE TABLE [dbo].[TimetableStationRegion]
(
    [TimetableStationId] INT NOT NULL,
    [RegionId]	         INT NOT NULL,

    CONSTRAINT [PK_TimetableStationRegion] PRIMARY KEY ([TimetableStationId], [RegionId]),
    CONSTRAINT [FK_TimetableStationRegion_TimetableStation] FOREIGN KEY ([TimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableStationRegion_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id]), 
)

