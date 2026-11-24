CREATE TABLE [dbo].[ModuleStandard] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [ShortName]      NVARCHAR (12)  NULL,
    [ScaleId]        INT            NOT NULL,
    [TrackSystem]    NVARCHAR (20)  NULL,
    [NormalGauge]    FLOAT (53)     NULL,
    [NarrowGauge]    FLOAT (53)     NULL,
    [Wheelset]       NVARCHAR (50)  NULL,
    [Couplings]      NVARCHAR (20)  NULL,
    [Electricity]    NVARCHAR (20)  NULL,
    [PreferredTheme] NVARCHAR (50)  NULL,
    [AcceptedNorm]   NVARCHAR (255) NULL,
    [MainThemeId] INT NOT NULL DEFAULT 0, 
    [ExtendsStandardId] INT NULL,
    [OnlyValidInCountryId] INT NULL,
    CONSTRAINT [PK_ModuleStandard] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleStandard_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id]),
    CONSTRAINT [FK_ModuleStandard_ExtendsStandard] FOREIGN KEY ([ExtendsStandardId]) REFERENCES [dbo].[ModuleStandard] ([Id]),
    CONSTRAINT [FK_ModuleStandard_OnlyValidInCountry] FOREIGN KEY ([OnlyValidInCountryId]) REFERENCES [dbo].[Country] ([Id])
);

