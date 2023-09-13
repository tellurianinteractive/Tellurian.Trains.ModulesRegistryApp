CREATE TABLE [dbo].[TimetableScheduleTrainPart]
/* This is a joint table for schedules for locos, wagon sets AND cargo wagon blocks */
(
    /* Common for all types of schedules */
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableScheduleId] INT NOT NULL,
    [FromDepartureId] INT NOT NULL,
    [ToArrivalId] INT NOT NULL,
    [PositionInTrain] TINYINT NOT NULL DEFAULT 1,
    [ShowConnectNote] BIT NOT NULL DEFAULT 1, /* Use loco, couple wagon, load cargo etc */
    [ShowDisconnectNote] BIT NOT NULL DEFAULT 1,
    
    /* Only Loco Schedule */
    [IsUnmanned] BIT NOT NULL DEFAULT 0, /* When a loco is unmanned on a train part, it cannot be assigned to a driver duty. */
    [GetAtStagingArea] BIT NOT NULL DEFAULT 0,
    [PutAtStagingArea] BIT NOT NULL DEFAULT 0,
    [ReverseLoco] BIT NOT NULL DEFAULT 0,
    [TurnLoco] BIT NOT NULL DEFAULT 0,

    /* Only Cargo Schedule */
    [TransferOriginTimetableStationId] INT NULL, /* The wagonset is for wagons orginating from this station only. */
    [TransferDestinationTimetableStationId] INT NULL, /* The wagonset has final destination at this station */
    [AndBefore] BIT NOT NULL DEFAULT 0, /* The wagonset is originating from departure station or transfer origin or before that */
    [AndBeyond] BIT NOT NULL DEFAULT 0, /* The wagonset is all wagons to the arrival station or transfer destination and beyond */
    [AndRegions] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to arrival station and regions attached to this station or transfer destination. */
    [AndLocalDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to all sub-stations under the arrival station or transfer destination */
    [AllDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset brings wagons from departure station or transfer origin til all destinations */
    [MaxNumberOfWagons] TINYINT NULL, /* Recommended maximum number of wagons in the wagonset. */

    CONSTRAINT [PK_TimetableScheduleTrainPart] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableScheduleTrainPart_TimetableSchedule] FOREIGN KEY ([TimetableScheduleId]) REFERENCES [dbo].[TimetableSchedule] ([Id]),
    CONSTRAINT [FK_TimetableScheduleTrainPart_Departure] FOREIGN KEY ([FromDepartureId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_TimetableScheduleTrainPart_Arrival] FOREIGN KEY ([ToArrivalId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferOriginTimetableStation] FOREIGN KEY ([TransferOriginTimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferDestinationTimetableStation] FOREIGN KEY ([TransferDestinationTimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]),
);
GO
CREATE NONCLUSTERED INDEX [IX_ScheduleTrainPart_FromDepartureId]
    ON [dbo].[TimetableScheduleTrainPart]([FromDepartureId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_ScheduleTrainPart_ToArrivalId]
    ON [dbo].[TimetableScheduleTrainPart]([ToArrivalId] ASC);

GO
    CREATE TRIGGER [DeleteTimetableScheduleTrainPart] ON [TimetableScheduleTrainPart] INSTEAD OF DELETE
    AS
    BEGIN
        DELETE FROM [DriverDutyScheduleLocoPart] WHERE TimetableScheduleLocoPartId IN (SELECT Id FROM DELETED)
        DELETE FROM [TimetableScheduleTrainPart] WHERE Id IN (SELECT Id FROM DELETED)
    END
