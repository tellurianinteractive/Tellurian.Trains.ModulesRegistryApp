CREATE TABLE [dbo].[ModuleExit] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [ModuleId]        INT           NOT NULL,
    [Label]           NVARCHAR (50) NOT NULL,
    [Direction]       INT           CONSTRAINT [DF_ModuleExit_Direction] DEFAULT ((0)) NOT NULL,
    [GableTypeId]     INT           NOT NULL,
    CONSTRAINT [PK_ModuleExit] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleExit_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ON DELETE CASCADE
);



