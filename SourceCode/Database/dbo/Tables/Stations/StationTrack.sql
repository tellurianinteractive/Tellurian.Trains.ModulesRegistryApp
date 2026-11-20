CREATE TABLE [dbo].[StationTrack] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [StationId]      INT           NOT NULL,
    [Designation]    NVARCHAR (5)  NOT NULL,
    [DisplayOrder]   SMALLINT      NOT NULL,
    [IsSiding]       BIT           NOT NULL,
    [IsScheduled]    BIT           NOT NULL,
    [IsLockable]     BIT           NOT NULL DEFAULT 0,
    [MaxTrainLength] FLOAT (53)    NOT NULL,
    [PlatformLength] FLOAT (53)    NULL,
    [SpeedLimit]     SMALLINT      NULL,
    [UsageNote]      NVARCHAR (50) NULL,
    [IsThroughTrack] BIT           NOT NULL DEFAULT 0,
    [DirectionId]    INT           NOT NULL DEFAULT ((10)),
    CONSTRAINT [PK_StationTrack] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StationTrack_Station] FOREIGN KEY ([StationId]) REFERENCES [dbo].[Station] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_StationTrack_StationId]
    ON [dbo].[StationTrack]([StationId] ASC);

