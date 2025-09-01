CREATE TABLE [dbo].[MeetingParticipant] (
    [Id]                        INT                IDENTITY (1, 1) NOT NULL,
    [PersonId]                  INT                NOT NULL,
    [MeetingId]                 INT                NOT NULL,
    [RegistrationTime]          DATETIMEOFFSET (7) NOT NULL,
    [CancellationTime]          DATETIMEOFFSET (7) NULL,
    [ArrivalTime]               DATETIME           NULL,
    [ParticipateDay1]           BIT                NOT NULL DEFAULT 0,
    [ParticipateDay2]           BIT                NOT NULL DEFAULT 0,
    [ParticipateDay3]           BIT                NOT NULL DEFAULT 0,
    [ParticipateDay4]           BIT                NOT NULL DEFAULT 0,
    [ParticipateDay5]           BIT                NOT NULL DEFAULT 0,
    [ParticipateDay6]           BIT                NOT NULL DEFAULT 0,
    [EarliestDepartureTime]     TIME(0) NULL, 
    [LatestArrivalTime]         TIME(0) NULL, 
    [LastModifiedDateTime] DATETIMEOFFSET NULL, 
    [ApprovedDateTime] DATETIMEOFFSET NULL, 
    CONSTRAINT [PK_MeetingParticipant] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingParticipant_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]),
    CONSTRAINT [FK_MeetingParticipant_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
);
GO
CREATE INDEX [IX_MeetingParticipant_MeetingId] ON [MeetingParticipant] ([MeetingId])
GO
CREATE TRIGGER [DeleteMeetingParticipant] ON [MeetingParticipant] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [LayoutParticipant] WHERE [MeetingParticipantId] IN (SELECT Id FROM DELETED)
    DELETE FROM [MeetingParticipant] WHERE Id IN (SELECT Id FROM DELETED)
END

