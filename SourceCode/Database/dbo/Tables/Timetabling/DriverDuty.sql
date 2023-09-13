CREATE TABLE [dbo].[DriverDuty]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [OperatorId] INT NULL,
    [Number] SMALLINT NOT NULL,
    [OperatingDayId] INT NOT NULL,
    [Difficulty] TINYINT NULL,
    [IsCancelled] BIT NOT NULL DEFAULT 0,
    [StartDayOffset] TINYINT NOT NULL DEFAULT 0,
    [OtherArrivalTime] TIME(0) NULL,
    [EndDayOffset] TINYINT NOT NULL DEFAULT 0,
    [OtherTime] TIME(0) NULL,
    [InstructionsMarkdown] NVARCHAR(1000) NULL,

    CONSTRAINT [PK_DriverDuty] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DriverDuty_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
    CONSTRAINT [FK_DriverDuty_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[VehicleOperator] ([Id]),
    CONSTRAINT [FK_DriverDuty_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),

)
