CREATE TABLE [dbo].[TimetableVehicle]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [OperatorId] INT NULL,
    [SpareNumber] TINYINT NOT NULL DEFAULT 0, /* Spare number */
    [RequiredClass] NVARCHAR(10) NULL,
    [IsTractionUnit] BIT NOT NULL DEFAULT 0,
    [IsDoubleDirectionOperation] BIT NOT NULL DEFAULT 0, /* Only applies to traction units */
    [AtTimetableStationId] INT NULL, /* For example a shuting loco placed at a specific station */
    [MaxNumberOfUnits] TINYINT NOT NULL DEFAULT 1, /* Wagonsets can consist of more that one unit. A loco can have several units operating as one loco. */
    [Description] NVARCHAR(50) NULL,

    CONSTRAINT [PK_TimetableVehicle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_TimetableVehicle_Number] UNIQUE ([SpareNumber], [TimetableId], [OperatorId]),
    CONSTRAINT [FK_TimetableVehicle_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
    CONSTRAINT [FK_TimetableVehicle_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[VehicleOperator] ([Id]),
    CONSTRAINT [FK_TimetableVehicle_TimetableStation] FOREIGN KEY ([AtTimetableStationId]) REFERENCES [dbo].[TimetableStation] ([Id]),

)
