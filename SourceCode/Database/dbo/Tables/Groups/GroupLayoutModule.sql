CREATE TABLE [dbo].[GroupLayoutModule]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [GroupId] INT NOT NULL,
    [ModuleId] INT NOT NULL,
    [GroupMemberId] INT NULL, 
    CONSTRAINT [PK_GroupLayoutModule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GroupLayoutModule_Module] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Module] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupLayoutModule_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_GroupLayoutModule_GroupMember] FOREIGN KEY ([GroupMemberId]) REFERENCES [dbo].[GroupMember] ([Id]) ON DELETE CASCADE
)
