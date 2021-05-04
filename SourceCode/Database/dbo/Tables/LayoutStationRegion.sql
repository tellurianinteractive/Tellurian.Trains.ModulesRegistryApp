CREATE TABLE [dbo].[LayoutStationRegion]
(
	[LayoutId]	INT NOT NULL,
	[RegionId]	INT NOT NULL,

	CONSTRAINT [FK_LayoutStationRegion_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_LayoutStationRegion_Region] FOREIGN KEY ([RegionId]) REFERENCES [dbo].[Region] ([Id]), 
    CONSTRAINT [PK_LayoutStationRegion] PRIMARY KEY ([LayoutId], [RegionId]),
)

