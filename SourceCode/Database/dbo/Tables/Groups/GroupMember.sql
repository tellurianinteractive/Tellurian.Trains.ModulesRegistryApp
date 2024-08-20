CREATE TABLE [dbo].[GroupMember] (
    [Id]                        INT IDENTITY (1, 1) NOT NULL,
    [GroupId]                   INT NOT NULL,
    [PersonId]                  INT NOT NULL,
    [IsGroupAdministrator]      BIT DEFAULT ((0)) NOT NULL,
    [IsDataAdministrator]       BIT DEFAULT ((0)) NOT NULL,
    [IsMeetingAdministrator]    BIT DEFAULT ((0)) NOT NULL,
    [MayBorrowGroupsModules]    BIT DEFAULT ((0)) NOT NULL,
    [MemberMayBorrowMyModules]  BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_GroupMember] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GroupMember_Group] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Group] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupMember_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id]) ON DELETE CASCADE
);



