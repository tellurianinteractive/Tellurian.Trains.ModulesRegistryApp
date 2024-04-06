CREATE TABLE [dbo].[Cargo] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [DefaultClasses] NVARCHAR (10) NULL,
    [FromYear]       SMALLINT      NULL,
    [UptoYear]       SMALLINT      NULL,
    [NHMCode]        INT           NULL,
    [DA]             NVARCHAR (50) NULL,
    [DE]             NVARCHAR (50) NULL,
    [EN]             NVARCHAR (50) NULL,
    [FR]             NVARCHAR (50) NULL,
    [IT]             NVARCHAR (50) NULL,
    [NL]             NVARCHAR (50) NULL,
    [NB]             NVARCHAR (50) NULL,
    [PL]             NVARCHAR (50) NULL,
    [SV]             NVARCHAR (50) NULL,
    [IsExpress] BIT NOT NULL DEFAULT 0, 
    [IsCoolingRequired] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_Cargo] PRIMARY KEY CLUSTERED ([Id] ASC)
);



