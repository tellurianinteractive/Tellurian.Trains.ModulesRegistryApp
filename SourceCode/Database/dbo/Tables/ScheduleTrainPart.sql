CREATE TABLE [dbo].[ScheduleTrainPart]
/* This is a joint table for schedules for locos, wagon sets AND cargo wagon blocks */
(
    /* Common for all types of schedules */
    [Id] INT NOT NULL IDENTITY (1, 1),
    [ScheduleId] INT NOT NULL,
    [FromDepartureId] INT NOT NULL,
    [ToArrivalId] INT NOT NULL,
    [PositionInTrain] TINYINT NOT NULL DEFAULT 1,
    [ShowAssignNote] BIT NOT NULL DEFAULT 1, /* Use loco, couple wagon, load cargo etc */
    [ShowDetachNote] BIT NOT NULL DEFAULT 1,
    
    /* Only Loco Schedule */
    [IsUnmanned] BIT NOT NULL DEFAULT 0, /* When a loco is unmanned on a train part, it cannot be assigned to a driver duty. */
    [GetAtParking] BIT NOT NULL DEFAULT 0,
    [PutAtParking] BIT NOT NULL DEFAULT 0,
    [ReverseLoco] BIT NOT NULL DEFAULT 0,
    [TurnLoco] BIT NOT NULL DEFAULT 0,

    /* Only Cargo Schedule */
    [TransferOriginLayoutStationId] INT NULL, /* The wagonset is for wagons orginating from this station only. */
    [TransferDestinationLayoutStationId] INT NULL, /* The wagonset has final destination at this station */
    [AndBefore] BIT NOT NULL DEFAULT 0, /* The wagonset is originating from departure station or transfer origin or before that */
    [AndBeyond] BIT NOT NULL DEFAULT 0, /* The wagonset is all wagons to the arrival station or transfer destination and beyond */
    [AndRegions] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to arrival station and regions attached to this station or transfer destination. */
    [AndLocalDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset contains wagons to all sub-stations under the arrival station or transfer destination */
    [AllDestinations] BIT NOT NULL DEFAULT 0, /* The wagonset brings wagons from departure station or transfer origin til all destinations */
    [MaxNumberOfWagons] TINYINT NULL, /* Recommended maximum number of wagons in the wagonset. */

    CONSTRAINT [PK_ScheduleTrainPart] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ScheduleTrainPart_Schedule] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[Schedule] ([Id]),
    CONSTRAINT [FK_ScheduleTrainPart_Departure] FOREIGN KEY ([FromDepartureId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_ScheduleTrainPart_Arrival] FOREIGN KEY ([ToArrivalId]) REFERENCES [dbo].[TrainStationCall] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferOriginLayoutStation] FOREIGN KEY ([TransferOriginLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
    CONSTRAINT [FK_CargoScheduleTrainPart_TransferDestinationLayoutStation] FOREIGN KEY ([TransferDestinationLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
);

GO
    CREATE TRIGGER [DeleteScheduleTrainPart] ON [ScheduleTrainPart] INSTEAD OF DELETE
    AS
    BEGIN
        DELETE FROM [DriverDutyScheduleLocoPart] WHERE ScheduleLocoPartId IN (SELECT Id FROM DELETED)
        DELETE FROM [ScheduleTrainPart] WHERE Id IN (SELECT Id FROM DELETED)
    END
