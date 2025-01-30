CREATE TABLE [dbo].[NHM] (
    [Id]          INT             NOT NULL,
    [MaxId]       INT             NOT NULL DEFAULT(0),
    [Code]        NCHAR (8)       NOT NULL,
    [LevelDigits] TINYINT         NOT NULL,
    [DA]          NVARCHAR (2000) NOT NULL,
    [DE]          NVARCHAR (2000) NOT NULL,
    [EN]          NVARCHAR (2000) NOT NULL,
    [NL]          NVARCHAR (2000) NOT NULL,
    [PL]          NVARCHAR (2000) NOT NULL,
    [SV]          NVARCHAR (2000) NOT NULL,
    [HasEmptyWagonOrder] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [PK_NHM] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO


