CREATE TABLE [dbo].[Layout] (
    [Id]                             INT            IDENTITY (1, 1) NOT NULL,
    [MeetingId]                      INT            NOT NULL,
    [OrganisingGroupId]              INT            NOT NULL,
    [ContactPersonId]                INT            NULL,
    [ShortName]                      NVARCHAR(10)   NULL,
    [Theme]                          NVARCHAR(100)  NULL,
    [PrimaryModuleStandardId]        INT            NOT NULL,
    [RegistrationClosingDate]        SMALLDATETIME  NOT NULL,
    [RegistrationOpeningDate]        SMALLDATETIME  NOT NULL,
    [ModuleRegistrationClosingDate]  SMALLDATETIME  NULL,
    [IsRegistrationPermitted]        BIT            NOT NULL DEFAULT 0,    
    [Details]                        NVARCHAR(MAX)  NULL,
    [FirstYear]                      SMALLINT       NULL,
    [LastYear]                       SMALLINT       NULL,
    [MaxNumberOfParticipants]        INT            NULL, 
    CONSTRAINT [PK_Layout] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Layout_OrganisingGroup] FOREIGN KEY ([OrganisingGroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_Layout_ResponsiblePerson] FOREIGN KEY ([ContactPersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [FK_Layout_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]),
    CONSTRAINT [FK_Layout_ModuleStandard] FOREIGN KEY ([PrimaryModuleStandardId]) REFERENCES [dbo].[ModuleStandard] ([Id])
);
GO
CREATE INDEX [IX_Layout_MeetingId] ON [Layout] ([MeetingId])
GO
CREATE TRIGGER [DeleteLayout] ON [Layout] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [LayoutParticipant] WHERE [LayoutId] IN (SELECT Id FROM DELETED)
    DELETE FROM [Layout] WHERE Id IN (SELECT Id FROM DELETED)
    
END

