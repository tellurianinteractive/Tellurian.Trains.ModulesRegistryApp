CREATE TABLE [dbo].[Layout] (
    [Id]                             INT            IDENTITY (1, 1) NOT NULL,
    [MeetingId]                      INT            NOT NULL,
    [OrganisingGroupId]              INT            NOT NULL,
    [ContactPersonId]                INT            NULL,
    [PrimaryModuleStandardId]        INT            NOT NULL,
    [Theme]                          NVARCHAR (100) NULL,
    [Details]                        NVARCHAR (MAX) NULL,
    [FirstYear]                      SMALLINT       NULL,
    [LastYear]                       SMALLINT       NULL,
    [StartWeekdayId]                 INT            NULL,
    [StartHour]                      SMALLINT       NULL,
    [EndHour]                        SMALLINT       NULL,
    [RegistrationClosingDate]        SMALLDATETIME  NOT NULL,
    [RegistrationOpeningDate]        SMALLDATETIME  NOT NULL,
    [ModuleRegistrationClosingDate]  SMALLDATETIME  NULL,
    CONSTRAINT [PK_Layout] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Layout_OrganisingGroup] FOREIGN KEY ([OrganisingGroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_Layout_ResponsiblePerson] FOREIGN KEY ([ContactPersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [FK_Layout_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]),
    CONSTRAINT [FK_Layout_ModuleStandard] FOREIGN KEY ([PrimaryModuleStandardId]) REFERENCES [dbo].[ModuleStandard] ([Id]),
    CONSTRAINT [FK_Layout_OperatingDay] FOREIGN KEY ([StartWeekdayId]) REFERENCES [dbo].[OperatingDay] ([Id])
);
GO
CREATE TRIGGER [DeleteLayout] ON [Layout] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [LayoutLine] WHERE [LayoutId] IN (SELECT Id FROM DELETED)
    DELETE FROM [LayoutParticipant] WHERE [LayoutId] IN (SELECT Id FROM DELETED)
    DELETE FROM [Layout] WHERE Id IN (SELECT Id FROM DELETED)
END

