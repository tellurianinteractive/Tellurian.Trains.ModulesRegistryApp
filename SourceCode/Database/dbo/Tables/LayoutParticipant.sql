CREATE TABLE [dbo].[LayoutParticipant] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [ParticipantId]    INT                NOT NULL,
    [LayoutId]         INT                NOT NULL,
    [RegistrationTime] DATETIMEOFFSET (7) NOT NULL,
    [CancellationTime] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_LayoutParticipant] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Layoutarticipant_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [dbo].[MeetingParticipant] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutParticipant_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id])
);

