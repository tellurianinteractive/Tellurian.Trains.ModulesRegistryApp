CREATE TABLE [dbo].[TimetableVehicleProvider]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableVehicleId] INT NOT NULL,
    [Position] TINYINT NOT NULL DEFAULT 0, /* When adding individual vehicles, they should be ordered according odd train number direction. */
    [ProvidingLayoutParticipantId] INT NOT NULL,
    [OtherClass] NVARCHAR(10) NULL, /* More detailed or deviation class from required */
    [SingleVehicleNumber] SMALLINT NULL, /* Only applies when TimetableVehicle.MaxNumberOfUnits is 1.*/
    [DccAddress] SMALLINT NULL, /* Only for decoder equipped vehicles */
    [CommentToPlanner] NVARCHAR(50) NULL,
    [CommentToProvider] NVARCHAR(50) NULL,
    CONSTRAINT [PK_TimetableVehicleProvider] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableVehicleProvider_TimetableVehicle] FOREIGN KEY ([TimetableVehicleId]) REFERENCES [dbo].[TimetableVehicle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableVehicleProvider_LayoutParticipant] FOREIGN KEY ([ProvidingLayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]),

)
