CREATE TABLE [dbo].[Person] (
    [Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [FirstName]           NVARCHAR (50) NOT NULL,
    [MiddleName]          NVARCHAR (50) NULL,
    [LastName]            NVARCHAR (50) NOT NULL,
    [EmailAddresses]      NVARCHAR (50) NULL,
    [CityName]            NVARCHAR (50) NULL,
    [CountryId]           INT           NOT NULL,
    [UserId]              INT           NULL,
    [FremoOwnerSignature] NVARCHAR (10) NULL,
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Person_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_Person_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);

