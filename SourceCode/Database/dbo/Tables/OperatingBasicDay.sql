CREATE TABLE [dbo].[OperatingBasicDay] (
    [OperatingDayId] INT NOT NULL,
    [BasicDayId]     INT NOT NULL,
    CONSTRAINT [PK_OperatingDayId_BasicDayId] PRIMARY KEY CLUSTERED ([OperatingDayId] ASC, [BasicDayId] ASC),
    CONSTRAINT [FK_OperatingBasicDay_BasicDays] FOREIGN KEY ([BasicDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_OperatingBasicDay_OperatingDays] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id])
);

