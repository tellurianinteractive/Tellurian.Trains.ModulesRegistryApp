CREATE TABLE [dbo].[Train]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableId] INT NOT NULL,
    [OperatorId] INT NULL,
    [Number] SMALLINT NOT NULL,
    [OperatingDayId] INT NOT NULL,
    [TrainCategoryId] INT NOT NULL,
    [TimetableRoutePatternId] INT NULL,
    [MaxSpeed] SMALLINT NOT NULL DEFAULT 100,
    [InstructionMarkdown] VARCHAR(1000) NULL,
    [Image] VARBINARY(MAX) NULL, 
    [Color] VARCHAR(8) NULL,
    CONSTRAINT [PK_Train] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Train_Timetable] FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetable] ([Id]),
    CONSTRAINT [FK_Train_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[VehicleOperator] ([Id]),
    CONSTRAINT [FK_Train_OperatingDay] FOREIGN KEY ([OperatingDayId]) REFERENCES [dbo].[OperatingDay] ([Id]),
    CONSTRAINT [FK_Train_TrainCategory] FOREIGN KEY ([TrainCategoryId]) REFERENCES [dbo].[TrainCategory] ([Id]),
    CONSTRAINT [FK_Train_TimetableRoutePattern] FOREIGN KEY ([TimetableRoutePatternId]) REFERENCES [dbo].[TimetableRoutePattern] ([Id]),
);
GO
CREATE NONCLUSTERED INDEX [IX_Train_TimetableId]
    ON [dbo].[Train]([TimetableId] ASC);

GO
    CREATE TRIGGER [DeleteTrain] ON [Train] INSTEAD OF DELETE 
    AS
    BEGIN
        DELETE FROM [TrainStationCall] WHERE TrainId IN (SELECT Id FROM DELETED)
        DELETE FROM [Train] WHERE Id IN (SELECT Id FROM DELETED)
    END
