CREATE TABLE [dbo].[ModuleFeature] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [ModuleStandardId] INT NOT NULL,
    [FeatureId] INT NOT NULL,
    CONSTRAINT [PK_ModuleFeature] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleFeature_ModuleStandard] FOREIGN KEY ([ModuleStandardId]) REFERENCES [dbo].[ModuleStandard] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ModuleFeature_Feature] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[Feature] ([Id])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ModuleFeature]
    ON [dbo].[ModuleFeature]([ModuleStandardId] ASC, [FeatureId] ASC);