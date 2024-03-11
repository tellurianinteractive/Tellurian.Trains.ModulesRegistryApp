CREATE TABLE [dbo].[ScheduleTrainPart]
/* This is a joint table for schedules for locos, wagon sets AND cargo wagon blocks */
(
    /* Common for all types of schedules */
    [Id] INT NOT NULL IDENTITY (1, 1),
    [ScheduleId] INT NOT NULL,
    [FromDepartureId] INT NOT NULL,
    [ToArrivalId] INT NOT NULL,
    [PositionInTrain] TINYINT NOT NULL DEFAULT 1, /* Zero means no wagon, only a card will be transported. */
    [ShowConnectNote] BIT NOT NULL DEFAULT 1, /* Use loco, couple wagon, load cargo etc */
    [ShowDisconnectNote] BIT NOT NULL DEFAULT 1, /* Examples when not 1: arriving at shadow yard, or same trainset continues in new train with same loco. */
    
    /* Additional for Loco Schedule only*/
    [IsUnmanned] BIT NOT NULL DEFAULT 0, /* When a loco is unmanned on a train part, it cannot be assigned to a driver duty. */
    [GetAtStagingArea] BIT NOT NULL DEFAULT 0, /* When true, a note is added to instruct driver to get loco from staging area and drive it to departure track. */
    [PutAtStagingArea] BIT NOT NULL DEFAULT 0, /* When true, a note is added to instruct driver to put loco at staging area after arrival.  */
    [ReverseLoco] BIT NOT NULL DEFAULT 0, /* After arrival, the loco should be reversed to the other end of the train using another track or loco lift. */
    [TurnLoco] BIT NOT NULL DEFAULT 0, /* After arrival, the loco should be turned using a turtablle or loco lift. */

    /* Additional for Cargo Schedule only */
    [TransferOriginTimetableStationId] INT NULL, /* The wagonset is for wagons orginating from this station only. */
    [TransferDestinationTimetableStationId] INT NULL, /* The wagonset has final destination at this station */
    [AndBefore] BIT NOT NULL DEFAULT 0, /* The wagonset is originating from departure station or before OR if transfer origin is not null from it and before */
    [AndBeyond] BIT NOT NULL DEFAULT 0, /* The wagonset is destinated to the arrival station and beyyond OR if transfer destination is not null to it and beyond */
    [AndRegions] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to arrival station and regions attached to this station OR if transfer destination is not null to it and regions attached to it.. */
    [AndLocalDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to all stations present in the current layout. */
    [AllDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset brings wagons from departure station or transfer origin to all destinations */
    [MaxNumberOfWagons] TINYINT NULL, /* Recommended maximum number of wagons in the wagonset. */

    CONSTRAINT [PK_TimetableScheduleTrainPart] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableScheduleTrainPart_TimetableSchedule] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[Schedule] ([Id]),
    CONSTRAINT [FK_TimetableScheduleTrainPart_Departure] FOREIGN KEY ([FromDepartureId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_TimetableScheduleTrainPart_Arrival] FOREIGN KEY ([ToArrivalId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferOriginTimetableStation] FOREIGN KEY ([TransferOriginTimetableStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferDestinationTimetableStation] FOREIGN KEY ([TransferDestinationTimetableStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
);
GO
CREATE NONCLUSTERED INDEX [IX_ScheduleTrainPart_FromDepartureId]
    ON [dbo].[ScheduleTrainPart]([FromDepartureId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_ScheduleTrainPart_ToArrivalId]
    ON [dbo].[ScheduleTrainPart]([ToArrivalId] ASC);

GO
    CREATE TRIGGER [DeleteTimetableScheduleTrainPart] ON [ScheduleTrainPart] INSTEAD OF DELETE
    AS
    BEGIN
        DELETE FROM [DriverDutyScheduleLocoPart] WHERE TimetableScheduleLocoPartId IN (SELECT Id FROM DELETED)
        DELETE FROM [ScheduleTrainPart] WHERE Id IN (SELECT Id FROM DELETED)
    END
