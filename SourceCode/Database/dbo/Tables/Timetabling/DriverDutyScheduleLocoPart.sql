CREATE TABLE [dbo].[DriverDutyScheduleLocoPart]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [DriverDutyId] INT NOT NULL,
    [TimetableScheduleLocoPartId] INT NOT NULL
    
    CONSTRAINT [PK_DriverDutyScheduleLocoPart] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DriverDutyScheduleLocoPart_DriverDuty] FOREIGN KEY ([DriverDutyId]) REFERENCES [dbo].[DriverDuty] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DriverDutyScheduleLocoPart_ScheduleTrainPart] FOREIGN KEY ([TimetableScheduleLocoPartId]) REFERENCES [dbo].[ScheduleTrainPart] ([Id])
    
)
GO

CREATE NONCLUSTERED INDEX [IX_DriverDutyScheduleLocoPart_TimetableScheduleLocoPartId]
    ON [dbo].[DriverDutyScheduleLocoPart](TimetableScheduleLocoPartId ASC);

GO
