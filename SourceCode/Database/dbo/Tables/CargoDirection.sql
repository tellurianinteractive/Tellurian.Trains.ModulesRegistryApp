CREATE TABLE [dbo].[CargoDirection] (
    [Id]        INT           NOT NULL,
    [FullName]  NVARCHAR (10) NOT NULL,
    [ShortName] NVARCHAR (4)  NOT NULL,
    [IsSupply]  BIT           NOT NULL,
    CONSTRAINT [PK_CargoDirection] PRIMARY KEY CLUSTERED ([Id] ASC)
);

