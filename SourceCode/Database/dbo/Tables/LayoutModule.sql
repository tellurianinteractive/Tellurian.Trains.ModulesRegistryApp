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
	CONSTRAINT [FK_LayoutModule_LayoutParticipant] FOREIGN KEY ([LayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_LayoutModule_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ,
	CONSTRAINT [FK_LayoutModule_LayoutStation] FOREIGN KEY ([LayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id])
)
