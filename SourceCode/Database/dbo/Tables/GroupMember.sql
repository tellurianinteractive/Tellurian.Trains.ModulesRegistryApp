CREATE TABLE [dbo].[GroupMember] (
    [Id]                   INT IDENTITY (1, 1) NOT NULL,
    [GroupId]              INT NOT NULL,
    [PersonId]             INT NOT NULL,
    [IsGroupAdministrator] BIT CONSTRAINT [DF_GroupMember_IsGroupAdministrator] DEFAULT ((0)) NOT NULL,
    [IsDataAdministrator]  BIT CONSTRAINT [DF_GroupMember_IsDataAdministrator] DEFAULT ((0)) NOT NULL,
    [MayBorrowModules]     BIT CONSTRAINT [DF_GroupMember_MayBorrowModules] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_GroupMember] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GroupMember_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupMember_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id]) ON DELETE CASCADE
);



