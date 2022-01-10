CREATE TABLE [dbo].[LayoutStationRegion]
(
    [LayoutStationId]	INT NOT NULL,
    [RegionId]	        INT NOT NULL,

    CONSTRAINT [PK_LayoutStationRegion] PRIMARY KEY ([LayoutStationId], [RegionId]),
    CONSTRAINT [FK_LayoutStationRegion_LayoutStation] FOREIGN KEY ([LayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LayoutStationRegion_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id]), 
)

