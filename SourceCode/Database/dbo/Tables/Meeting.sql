CREATE TABLE [dbo].[Meeting] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [OrganiserGroupId] INT           NOT NULL,
    [PlaceName]        NVARCHAR (50) NOT NULL,
    [Name]             NVARCHAR (50) NOT NULL,
    [StartDate]        DATETIME      NOT NULL,
    [EndDate]          DATETIME      NOT NULL,
    [Status]           INT           NOT NULL,
    [IsFremo]          BIT           CONSTRAINT [DF_Meeting_IsFremoMeeting] DEFAULT ((0)) NOT NULL,
    [Accomodation]    NVARCHAR(MAX) NULL, 
    [Details]      NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_Meeting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Meeting_Group] FOREIGN KEY ([OrganiserGroupId]) REFERENCES [dbo].[Group] ([Id])
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


