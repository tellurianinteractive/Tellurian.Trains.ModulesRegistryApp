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
    [MainTheme] NVARCHAR(10) NULL, 
    CONSTRAINT [PK_ModuleStandard] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ModuleStandard_Scale] FOREIGN KEY ([ScaleId]) REFERENCES [dbo].[Scale] ([Id])
);

