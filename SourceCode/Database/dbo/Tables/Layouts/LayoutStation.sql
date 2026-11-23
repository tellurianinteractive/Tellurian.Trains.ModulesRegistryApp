CREATE TABLE [dbo].[LayoutStation]
(
    [Id]						  INT NOT NULL PRIMARY KEY IDENTITY,
    [LayoutParticipantId]         INT NOT NULL,
    [StationId]					  INT NOT NULL,
    [OtherName]					  NVARCHAR(50) NULL,
    [OtherSignature]			  NVARCHAR(5) NULL,
    [OtherCountryId]			  INT NULL,
    CONSTRAINT [FK_LayoutStation_LayoutParticipant] FOREIGN KEY ([LayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutStation_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]),
    CONSTRAINT [FK_LayoutStation_Country] FOREIGN KEY ([OtherCountryId]) REFERENCES [dbo].[Country] ([Id])
)
GO
CREATE INDEX [IX_LayoutStation_LayoutParticipantId] ON [LayoutStation] ([LayoutParticipantId])
GO
CREATE INDEX [IX_LayoutStation_StationId] ON [LayoutStation] (StationId)
GO
