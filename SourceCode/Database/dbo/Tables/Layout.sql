CREATE TABLE [dbo].[Layout] (
    [Id]                      INT            IDENTITY (1, 1) NOT NULL,
    [MeetingId]               INT            NOT NULL,
    [ResponsibleGroupId]      INT            NOT NULL,
    [PrimaryModuleStandardId] INT            NOT NULL,
    [Theme]                   NVARCHAR (100) NULL,
    [Note]                    NVARCHAR (100) NULL,
    [FirstYear]               SMALLINT       NULL,
    [LastYear]                SMALLINT       NULL,
    [StartWeekdayId]          INT            NULL,
    [StartHour]               SMALLINT       NULL,
    [EndHour]                 SMALLINT       NULL,
    [RegistrationClosingDate] SMALLDATETIME  NOT NULL,
    [RegistrationOpeningDate] SMALLDATETIME  NOT NULL,
    CONSTRAINT [PK_Layout] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Layout_Group] FOREIGN KEY ([ResponsibleGroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_Layout_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Layout_ModuleStandard] FOREIGN KEY ([PrimaryModuleStandardId]) REFERENCES [dbo].[ModuleStandard] ([Id]),
    CONSTRAINT [FK_Layout_OperatingDay] FOREIGN KEY ([StartWeekdayId]) REFERENCES [dbo].[OperatingDay] ([Id])
);

