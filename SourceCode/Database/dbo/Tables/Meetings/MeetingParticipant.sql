CREATE TABLE [dbo].[MeetingParticipant] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [PersonId]         INT                NOT NULL,
    [MeetingId]        INT                NOT NULL,
    [RegistrationTime] DATETIMEOFFSET (7) NOT NULL,
    [CancellationTime] DATETIMEOFFSET (7) NULL,
    [ArrivalTime]      DATETIME           NULL,
    [ParticipateDay1]  BIT                NOT NULL,
    [ParticipateDay2]  BIT                NOT NULL,
    [ParticipateDay3]  BIT                NOT NULL,
    [ParticipateDay4]  BIT                NOT NULL,
    [ParticipateDay5]  BIT                NOT NULL,
    CONSTRAINT [PK_MeetingParticipant] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingParticipant_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]),
    CONSTRAINT [FK_MeetingParticipant_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
);
GO
CREATE TRIGGER [DeleteMeetingParticipant] ON [MeetingParticipant] INSTEAD OF DELETE 
AS
BEGIN
    DELETE FROM [LayoutParticipant] WHERE MeetingParticipantId IN (SELECT Id FROM DELETED)
    DELETE FROM [MeetingParticipant] WHERE Id IN (SELECT Id FROM DELETED)
END
