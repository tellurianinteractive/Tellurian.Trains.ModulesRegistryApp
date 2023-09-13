CREATE TABLE [dbo].[TrainStationCall]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TrainId] INT NOT NULL,
    [StationTrackId] INT NOT NULL,
    [ArrivalDayOffset] TINYINT NOT NULL DEFAULT 0,
    [ArrivalTime] TIME(0) NULL,
    [DepartureDayOffset] TINYINT NOT NULL DEFAULT 0,
    [DepartureTime] TIME(0) NOT NULL,
    [IsStopping] BIT NOT NULL DEFAULT 1,
    [HideMeets] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_TrainStationCall] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TrainStationCall_Train] FOREIGN KEY ([TrainId]) REFERENCES [dbo].[Train] ([Id]),
    CONSTRAINT [FK_TrainStationCall_StationTrack] FOREIGN KEY ([StationTrackId]) REFERENCES [dbo].[StationTrack] ([Id])
);
    GO
    CREATE TRIGGER [DeleteTrainStationCall] ON [TrainStationCall] INSTEAD OF DELETE
    AS
    BEGIN
        DELETE FROM [TimetableScheduleTrainPart] WHERE FromDepartureId IN (SELECT Id FROM DELETED)
        DELETE FROM [TimetableScheduleTrainPart] WHERE ToArrivalId IN (SELECT Id FROM DELETED)
        DELETE FROM [TrainStationCall] WHERE Id IN (SELECT Id FROM DELETED)
    END
   
