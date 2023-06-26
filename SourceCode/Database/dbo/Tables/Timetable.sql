CREATE TABLE [dbo].[Timetable]
(
    [Id]                   INT IDENTITY (1, 1) NOT NULL,
    [Name]                 NVARCHAR (100) NULL,
    [FirstOperatingDayId]  INT NULL,
    CONSTRAINT [PK_Timetable] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Timetable_FirstOperatingDay] FOREIGN KEY ([FirstOperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
)
