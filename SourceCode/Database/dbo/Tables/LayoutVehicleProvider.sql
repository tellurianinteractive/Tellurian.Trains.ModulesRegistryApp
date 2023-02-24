CREATE TABLE [dbo].[LayoutVehicleProvider]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutVehicleId] INT NOT NULL,
    [Position] TINYINT NOT NULL DEFAULT 0, /* When adding individual vehicles, they should be ordered according odd train number direction. */
    [ProvidingLayoutParticipantId] INT NOT NULL,
    [OtherClass] NVARCHAR(10) NULL, /* More detailed or deviation class from required */
    [SingleVehicleNumber] SMALLINT NULL, /* Only applies when LayoutVehicle.MaxNumberOfUnits is 1.*/
    [DccAddress] SMALLINT NULL, /* Only for decoder equippped vehicles */
    [CommentToPlanner] NVARCHAR(50) NULL,
    [CommentToProvider] NVARCHAR(50) NULL,
    CONSTRAINT [PK_LayoutVehicleProvider] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LayoutVehicleProvider_LayoutVehicle] FOREIGN KEY ([LayoutVehicleId]) REFERENCES [dbo].[LayoutVehicle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutVehicleProvider_LayoutParticipant] FOREIGN KEY ([ProvidingLayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]),

)
