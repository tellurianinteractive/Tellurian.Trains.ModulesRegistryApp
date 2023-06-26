CREATE TABLE [dbo].[TimetableVehicleSchedule]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableVehicleId] INT NOT NULL,
    [TimetableScheduleId] INT NOT NULL,
    [OperatingDayId] INT NOT NULL,
    CONSTRAINT [PK_TimetableVehicleSchedule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableVehicleSchedule_TrainVehicle] FOREIGN KEY ([TimetableVehicleId]) REFERENCES [dbo].[TimetableVehicle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableVehicleSchedule_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_TimetableVehicleSchedule_Schedule] FOREIGN KEY ([TimetableScheduleId]) REFERENCES [dbo].[TimetableSchedule] ([Id]),
)
