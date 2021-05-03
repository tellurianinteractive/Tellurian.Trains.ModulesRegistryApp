CREATE TABLE [dbo].[Group] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [CountryId]     INT           NOT NULL,
    [FullName]      NVARCHAR (50) NOT NULL,
    [ShortName]     NVARCHAR (10) NULL,
    [Category]      NVARCHAR (20) NOT NULL,
    [CityName]      NVARCHAR (50) NULL,
    [GroupDomainId] INT           NULL,
    CONSTRAINT [PK_Group] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Group_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_Group_GroupDomain] FOREIGN KEY ([GroupDomainId]) REFERENCES [dbo].[GroupDomain] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Group_CountryId]
    ON [dbo].[Group]([CountryId] ASC);

