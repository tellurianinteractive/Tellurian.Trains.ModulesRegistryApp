CREATE TABLE [dbo].[LayoutVehicleSchedule]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutVehicleId] INT NOT NULL,
    [OperatingDayId] INT NOT NULL,
    [ScheduleId] INT NOT NULL,
    CONSTRAINT [PK_LayoutVehicleSchedule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LayoutLayoutVehicleSchedule_TrainVehicle] FOREIGN KEY ([LayoutVehicleId]) REFERENCES [dbo].[LayoutVehicle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutVehicleSchedule_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_LayoutVehicleSchedule_Schedule] FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[Schedule] ([Id]),
)
