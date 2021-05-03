CREATE TABLE [dbo].[CargoUnit] (
    [Id]          INT           NOT NULL,
    [FullName]    NVARCHAR (12) NOT NULL,
    [Designation] NVARCHAR (6)  NOT NULL,
    CONSTRAINT [PK_CargoUnit] PRIMARY KEY CLUSTERED ([Id] ASC)
);

