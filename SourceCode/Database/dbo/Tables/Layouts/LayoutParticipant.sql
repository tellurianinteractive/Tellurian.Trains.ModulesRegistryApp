CREATE TABLE [dbo].[LayoutParticipant] (
    [Id]                    INT                IDENTITY (1, 1) NOT NULL,
    [PersonId]              INT                NOT NULL,
    [MeetingParticipantId]  INT                NOT NULL,
    [LayoutId]              INT                NOT NULL,
    [NoteToOrganiserMarkdown] NVARCHAR(MAX) NULL, 
    [BringsModulesAfterAgreement] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_LayoutParticipant] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LayoutParticipant_MeetingParticipant] FOREIGN KEY ([MeetingParticipantId]) REFERENCES [dbo].[MeetingParticipant] ([Id]),
    CONSTRAINT [FK_LayoutParticipant_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [FK_LayoutParticipant_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id])
);
GO
CREATE INDEX [IX_LayoutParticipant_LayoutId] ON [LayoutParticipant] ([LayoutId])
GO
CREATE INDEX [IX_LayoutParticipant_MeetingParticipantId] ON [LayoutParticipant] ([MeetingParticipantId])
GO

CREATE TRIGGER [DeleteLayoutParticipant] ON [dbo].[LayoutParticipant] INSTEAD OF DELETE
AS
BEGIN
    DELETE FROM [LayoutModule] WHERE [LayoutParticipantId] IN (SELECT Id FROM DELETED)
    DELETE FROM [LayoutStation] WHERE [LayoutParticipantId] IN (SELECT Id FROM DELETED)
    DELETE FROM [LayoutParticipant] WHERE Id IN (SELECT Id FROM DELETED)
END

