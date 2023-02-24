CREATE TABLE [dbo].[TrainCategory]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [IsFreight] BIT NOT NULL DEFAULT 0,
    [IsPassenger] BIT NOT NULL DEFAULT 0,
    [NumberPrefix] NVARCHAR(5) NULL,
    [NumberSuffix] NVARCHAR(5) NULL,
    [TranslationResourceCode] NVARCHAR(50) NOT NULL,
    [ColorHexCode] CHAR(6) NULL,
    CONSTRAINT [PK_TrainCategory] PRIMARY KEY CLUSTERED ([Id] ASC),

)
