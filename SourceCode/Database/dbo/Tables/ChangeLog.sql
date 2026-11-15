CREATE TABLE [dbo].[ChangeLog] (
    [Id]                INT                IDENTITY (1, 1) NOT NULL,
    [EntityType]        NVARCHAR (50)      NOT NULL,
    [EntityId]          INT                NOT NULL,
    [Operation]         TINYINT            NOT NULL,
    [ChangedProperties] NVARCHAR (1000)    NULL,
    [UserId]            INT                NULL,
    [ChangedAt]         DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_ChangeLog] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_ChangeLog_Operation] CHECK ([Operation] IN (1, 2, 3))
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeLog_Entity]
    ON [dbo].[ChangeLog]([EntityType] ASC, [EntityId] ASC, [ChangedAt] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeLog_UserId]
    ON [dbo].[ChangeLog]([UserId] ASC, [ChangedAt] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeLog_ChangedAt]
    ON [dbo].[ChangeLog]([ChangedAt] ASC);