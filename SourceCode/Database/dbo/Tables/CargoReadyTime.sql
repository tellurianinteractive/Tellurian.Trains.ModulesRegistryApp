CREATE TABLE [dbo].[CargoReadyTime] (
    [Id]                  INT           NOT NULL,
    [FullName]            NVARCHAR (20) NOT NULL,
    [ShortName]           NVARCHAR (10) NOT NULL,
    [IsSpecifiedInLayout] BIT           CONSTRAINT [DF_CargoReadyTime_IsSpecifiedInLayoyt] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_CargoReadyTime] PRIMARY KEY CLUSTERED ([Id] ASC)
);

