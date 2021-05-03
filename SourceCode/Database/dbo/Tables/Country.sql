CREATE TABLE [dbo].[Country] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [EnglishName]  NVARCHAR (50) NOT NULL,
    [DomainSuffix] NCHAR (2)     NOT NULL,
    [Languages]    NVARCHAR (10) NULL,
    [TimeZoneName] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'A semicolon separated list of two-letter language codes.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Country', @level2type = N'COLUMN', @level2name = N'Languages';

