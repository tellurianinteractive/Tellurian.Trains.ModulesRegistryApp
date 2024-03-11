CREATE TABLE [dbo].[TimetabledVehicle]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [OperatorId] INT NULL,
    [SpareNumber] TINYINT NOT NULL DEFAULT 0, -- Spare vehicles are 1 and above.
    [RequiredClass] NVARCHAR(10) NULL, -- Can be a complete class or just the initial main class letter.
    [IsTractionUnit] BIT NOT NULL DEFAULT 0,
    [IsDoubleDirectionOperation] BIT NOT NULL DEFAULT 0, -- Only applies to traction units with or without trainsets with control car.
    [AtLayoutStationId] INT NULL, -- For example a shuting loco placed at a specific station.
    [MaxNumberOfUnits] TINYINT NOT NULL DEFAULT 1, -- Wagonsets can consist of more that one unit. A loco can have several units operating as one loco.
    [Description] NVARCHAR(50) NULL,

    CONSTRAINT [PK_TimetabledVehicle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_TimetabledVehicle_Number] UNIQUE ([SpareNumber], [TimetableId], [OperatorId]),
    CONSTRAINT [FK_TimetableVehicle_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
    CONSTRAINT [FK_TimetableVehicle_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[VehicleOperator] ([Id]),
    CONSTRAINT [FK_TimetableVehicle_LayoutStation] FOREIGN KEY ([AtLayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),

)
