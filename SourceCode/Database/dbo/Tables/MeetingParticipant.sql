CREATE TABLE [dbo].[MeetingParticipant] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [PersonId]         INT                NOT NULL,
    [MeetingId]        INT                NOT NULL,
    [RegistrationTime] DATETIMEOFFSET (7) NOT NULL,
    [CancellationTime] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_MeetingParticipant] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingParticipant_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]),
    CONSTRAINT [FK_MeetingParticipant_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
);

