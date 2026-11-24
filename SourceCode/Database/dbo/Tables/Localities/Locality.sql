CREATE TABLE [dbo].[Locality] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [CountryId]         INT             NOT NULL,
    [Name]              NVARCHAR (100)  NOT NULL,
    [IsModuleStorage]   BIT             DEFAULT ((0)) NOT NULL,
    [IsMeetingVenue]    BIT             DEFAULT ((0)) NOT NULL,
    [StreetAddress]     NVARCHAR (100)  NULL,
    [PostalCode]        NVARCHAR (10)   NULL,
    [CityName]          NVARCHAR (50)   NOT NULL,
    [Latitude]          DECIMAL (9, 6)  NULL,
    [Longitude]         DECIMAL (9, 6)  NULL,
    [Description]       NVARCHAR (MAX)  NULL,
    [UsedByGroupId]     INT             NULL,
    [HallDrawingId]     INT             NULL,
    CONSTRAINT [PK_Locality] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Locality_Country] FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_Locality_UsedByGroup] FOREIGN KEY ([UsedByGroupId]) REFERENCES [dbo].[Group] ([Id]),
    CONSTRAINT [FK_Locality_HallDrawing] FOREIGN KEY ([HallDrawingId]) REFERENCES [dbo].[Document] ([Id])
);

GO
CREATE NONCLUSTERED INDEX [IX_Locality_CountryId]
    ON [dbo].[Locality]([CountryId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Locality_UsedByGroupId]
    ON [dbo].[Locality]([UsedByGroupId] ASC);