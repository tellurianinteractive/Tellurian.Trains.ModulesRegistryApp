CREATE TABLE [dbo].[Country] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [EnglishName]  NVARCHAR (50) NOT NULL,
    [DomainSuffix] NCHAR (2)     NOT NULL,
    [Languages]    NVARCHAR (10) NULL,
    [TimeZoneName] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO


