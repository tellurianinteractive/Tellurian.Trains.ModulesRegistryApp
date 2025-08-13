CREATE TABLE [dbo].[LayoutModule]
(
	[Id]                   INT NOT NULL PRIMARY KEY IDENTITY, 
	[LayoutParticipantId]  INT NOT NULL,
	[ModuleId]             INT NOT NULL,
	[LayoutStationId]      INT NULL,
	[RegisteredTime]       DATETIMEOFFSET(7) CONSTRAINT [DF_LayoutModule_RegisteredTime] DEFAULT ((SYSDATETIMEOFFSET())) NOT NULL,
	[RegistrationStatus]   INT NOT NULL,
	[BringAnyway]          BIT CONSTRAINT [DF_LayoutModule_BringAnyway] DEFAULT ((0)) NOT NULL,
	[Note]                 NVARCHAR(50) NULL,
	CONSTRAINT [FK_LayoutModule_LayoutParticipant] FOREIGN KEY ([LayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]),
	CONSTRAINT [FK_LayoutModule_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ,
	CONSTRAINT [FK_LayoutModule_LayoutStation] FOREIGN KEY ([LayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id])
)

GO
CREATE INDEX [IX_LayoutModule_LayoutParticipantId] ON [LayoutModule] ([LayoutParticipantId])
GO
CREATE INDEX [IX_LayoutModule_LayoutStationId] ON [LayoutModule] ([LayoutStationId])
GO
CREATE INDEX [IX_LayoutModule_ModuleId] ON [LayoutModule] ([ModuleId])
GO
CREATE TRIGGER [DeleteLayoutModule] ON [LayoutModule] INSTEAD OF DELETE
AS
BEGIN
	DELETE FROM [LayoutModule] WHERE ISNULL([LayoutStationId],0) IN (SELECT [LayoutStationId] FROM DELETED)
	DELETE FROM [LayoutModule] WHERE Id IN (SELECT Id FROM DELETED)
	DELETE FROM [LayoutStation] WHERE Id IN (SELECT [LayoutStationId] FROM DELETED)
END
GO

