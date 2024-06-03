CREATE TABLE [dbo].[TrainStationCall]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TrainId] INT NOT NULL,
    [StationTrackId] INT NOT NULL, -- Refers to Station, layout specific information about station tracks can optionally be added in LayoutStationTrack.
    [ArrivalDayOffset] TINYINT NOT NULL DEFAULT 0,
    [ArrivalTime] TIME(0) NULL,
    [DepartureDayOffset] TINYINT NOT NULL DEFAULT 0,
    [DepartureTime] TIME(0) NOT NULL,
    [IsExchange] BIT NOT NULL DEFAULT 1, -- If true, train stops for exchange of passengers and/or goods.
    [HideMeets] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_TrainStationCall] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TrainStationCall_Train] FOREIGN KEY ([TrainId]) REFERENCES [dbo].[Train] ([Id]),
    CONSTRAINT [FK_TrainStationCall_StationTrack] FOREIGN KEY ([StationTrackId]) REFERENCES [dbo].[StationTrack] ([Id])
);
GO
    CREATE NONCLUSTERED INDEX [IX_TrainStationCall_TrainId] ON
        [dbo].[TrainStationCall]( TrainId ASC);
GO
    CREATE TRIGGER [DeleteTrainStationCall] ON [TrainStationCall] INSTEAD OF DELETE
    AS
    BEGIN
        DELETE FROM [ScheduleTrainPart] WHERE FromDepartureId IN (SELECT Id FROM DELETED)
        DELETE FROM [ScheduleTrainPart] WHERE ToArrivalId IN (SELECT Id FROM DELETED)
        DELETE FROM [TrainStationCall] WHERE Id IN (SELECT Id FROM DELETED)
    END

   
