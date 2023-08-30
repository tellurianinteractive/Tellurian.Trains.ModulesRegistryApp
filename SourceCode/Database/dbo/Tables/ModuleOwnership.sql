CREATE TABLE [dbo].[ModuleOwnership] (
    [Id]                    INT IDENTITY (1, 1) NOT NULL,
    [PersonId]              INT NULL,
    [GroupId]               INT NULL,
    [ModuleId]              INT NOT NULL,
    [OwnedShare]            FLOAT (53) DEFAULT ((1)) NOT NULL
    CONSTRAINT [PK_ModuleOwnership] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleOwnership_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_ModuleOwnership_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ModuleOwnership_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_PersonId]
    ON [dbo].[ModuleOwnership]([PersonId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_ModuleId]
    ON [dbo].[ModuleOwnership]([ModuleId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_GroupId]
    ON [dbo].[ModuleOwnership]([GroupId] ASC);

