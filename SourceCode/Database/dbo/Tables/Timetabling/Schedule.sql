CREATE TABLE [dbo].[TimetableSchedule]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [Number] SMALLINT NOT NULL, /* Visible Identity of Schedule */
    [IsCargo] BIT NOT NULL DEFAULT 0, /* Because cargo schedules may not have a vehicle attached */
    CONSTRAINT [PK_TimetableSchedule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [AK_TimetableSchedule_Number] UNIQUE ([Number], [TimetableId]),
    CONSTRAINT [FK_TimetableSchedule_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
)
