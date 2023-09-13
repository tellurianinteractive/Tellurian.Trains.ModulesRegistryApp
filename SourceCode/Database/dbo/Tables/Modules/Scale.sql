CREATE TABLE [dbo].[Scale] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [ShortName]   NVARCHAR (10) NOT NULL,
    [Denominator] INT           NOT NULL,
    CONSTRAINT [PK_Scale] PRIMARY KEY CLUSTERED ([Id] ASC)
);

