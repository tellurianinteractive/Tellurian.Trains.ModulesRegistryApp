CREATE TABLE [dbo].[VehicleProvider]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [TimetableVehicleId] INT NOT NULL,
    [ProvidingLayoutParticipantId] INT NOT NULL,
    [Position] TINYINT NOT NULL DEFAULT 0, 
    [OtherClass] NVARCHAR(10) NULL, -- More detailed or deviation class from required.
    [SingleVehicleNumber] VARCHAR(20) NULL, -- Only applies when TimetableVehicle.MaxNumberOfUnits is 1.
    [DccAddress] SMALLINT NULL, -- Only for decoder equipped vehicles.
    [CommentToPlanner] NVARCHAR(100) NULL,
    [CommentToProvider] NVARCHAR(100) NULL,
    [ThrottleIdentity] NVARCHAR(50) NULL, /* Used for assignment of wiFRED throttles to control this loco */ 
    [LastModifiedDateTime] DATETIMEOFFSET NULL, 
    

    CONSTRAINT [PK_TimetableVehicleProvider] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimetableVehicleProvider_TimetableVehicle] FOREIGN KEY ([TimetableVehicleId]) REFERENCES [dbo].[TimetabledVehicle] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TimetableVehicleProvider_LayoutParticipant] FOREIGN KEY ([ProvidingLayoutParticipantId]) REFERENCES [dbo].[LayoutParticipant] ([Id]),
)
