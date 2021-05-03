CREATE TABLE [dbo].[Cargo] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [DefaultClasses] NVARCHAR (10) NULL,
    [FromYear]       SMALLINT      NULL,
    [UptoYear]       SMALLINT      NULL,
    [NHMCode]        INT           NOT NULL,
    [DA]             NVARCHAR (50) NULL,
    [DE]             NVARCHAR (50) NULL,
    [EN]             NVARCHAR (50) NULL,
    [NL]             NVARCHAR (50) NULL,
    [NO]             NVARCHAR (50) NULL,
    [PL]             NVARCHAR (50) NULL,
    [SV]             NVARCHAR (50) NULL,
    CONSTRAINT [PK_Cargo] PRIMARY KEY CLUSTERED ([Id] ASC)
);

