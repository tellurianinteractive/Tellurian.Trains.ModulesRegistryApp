CREATE TABLE [dbo].[LayoutVehicle]
(
    /* A vehicle is considered part of a session when associated with a schedule, or is placed at a layout station; otherwise the vehicle is optional */
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutId] INT NOT NULL,
    [OperatorId] INT NULL,
    [Number] TINYINT NOT NULL,
    [RequiredClass] NVARCHAR(10) NULL,
    [IsTractionUnit] BIT NOT NULL DEFAULT 0,
    [IsDoubleDirectionOperation] BIT NOT NULL DEFAULT 0,
    [AtLayoutStationId] INT NULL, /* For example a shuting loco placed at a specific station */
    [MaxNumberOfUnits] TINYINT NOT NULL DEFAULT 1, /* Wagonsets can consist of more that one unit. A loco can have several units operating as one loco. */
    [Description] NVARCHAR(50) NULL,

    CONSTRAINT [PK_LayoutVehicle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_LayoutVehicle_Number] UNIQUE ([Number], [LayoutId], [OperatorId]),
    CONSTRAINT [FK_LayoutVehicle_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),
    CONSTRAINT [FK_LayoutVehicle_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Operator] ([Id]),

)
