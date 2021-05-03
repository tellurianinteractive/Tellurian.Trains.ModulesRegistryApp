CREATE TABLE [dbo].[ModuleOwnership] (
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    [PersonId]   INT NULL,
    [GroupId]    INT NULL,
    [ModuleId]   INT NOT NULL,
    [OwnedShare] INT DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_ModuleOwnership] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleOwnership_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_ModuleOwnership_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ModuleOwnership_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
);




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Owning person (if null, an Organisation must own it)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ModuleOwnership', @level2type = N'COLUMN', @level2name = N'PersonId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Owning organisation (if null, a Person must own it)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ModuleOwnership', @level2type = N'COLUMN', @level2name = N'GroupId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The ownerships share as 1/this value.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ModuleOwnership', @level2type = N'COLUMN', @level2name = N'OwnedShare';


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_PersonId]
    ON [dbo].[ModuleOwnership]([PersonId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_ModuleId]
    ON [dbo].[ModuleOwnership]([ModuleId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ModuleOwnership_GroupId]
    ON [dbo].[ModuleOwnership]([GroupId] ASC);

