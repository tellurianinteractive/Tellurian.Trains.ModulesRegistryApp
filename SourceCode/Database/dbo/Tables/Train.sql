CREATE TABLE [dbo].[Train]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutId] INT NOT NULL,
    [OperatorId] INT NULL,
    [Number] SMALLINT NOT NULL,
    [OperatingDayId] INT NOT NULL,
    [TrainCategoryId] INT NOT NULL,
    [LayoutRoutePatternId] INT NULL,
    [MaxSpeed] SMALLINT NOT NULL DEFAULT 100,
    [InstructionMarkdown] VARCHAR(1000) NULL,
    [Image] VARBINARY(MAX) NULL, 
    CONSTRAINT [PK_Train] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Train_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),
    CONSTRAINT [FK_Train_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Operator] ([Id]),
    CONSTRAINT [FK_Train_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_Train_TrainCategory] FOREIGN KEY ([TrainCategoryId]) REFERENCES [dbo].[TrainCategory] ([Id]),
    CONSTRAINT [FK_Train_LayoutRoutePattern] FOREIGN KEY ([LayoutRoutePatternId]) REFERENCES [dbo].[LayoutRoutePattern] ([Id]),
);
GO
CREATE NONCLUSTERED INDEX [IX_Train_LayoutId]
    ON [dbo].[Train]([LayoutId] ASC);

GO
    CREATE TRIGGER [DeleteTrain] ON [Train] INSTEAD OF DELETE 
    AS
    BEGIN
        DELETE FROM [TrainStationCall] WHERE TrainId IN (SELECT Id FROM DELETED)
        DELETE FROM [Train] WHERE Id IN (SELECT Id FROM DELETED)
    END
