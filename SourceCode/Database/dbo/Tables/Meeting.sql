CREATE TABLE [dbo].[Meeting] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [OrganiserGroupId] INT           NOT NULL,
    [CityName]         NVARCHAR (50) NOT NULL,
    [VenueName]        NVARCHAR (50) NULL,
    [Name]             NVARCHAR (50) NOT NULL,
    [StartDate]        DATETIME      NOT NULL,
    [EndDate]          DATETIME      NOT NULL,
    [Status]           INT           NOT NULL,
    [GroupDomainId]    INT           NULL,
    [Accomodation]     NVARCHAR(MAX) NULL, 
    [Details]          NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_Meeting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Meeting_Group] FOREIGN KEY ([OrganiserGroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_Meeting_GroupDomain] FOREIGN KEY ([GroupDomainId]) REFERENCES [dbo].[GroupDomain] ([Id])

);
GO
CREATE TRIGGER [DeleteMeeting] ON [Meeting] INSTEAD OF DELETE
AS
BEGIN
    DELETE FROM [MeetingParticipant] WHERE MeetingId IN (SELECT Id FROM DELETED)
    DELETE FROM [Layout] WHERE MeetingId IN (SELECT Id FROM DELETED)
    DELETE FROM [Meeting] WHERE Id IN (SELECT Id FROM DELETED)
END
GO


