CREATE TABLE [dbo].[LayoutRoutePatternLayoutStation]
(
	[Id] INT NOT NULL IDENTITY (1, 1),
	[LayoutRoutePatternId] INT NOT NULL,
	[LayoutStationId] INT NOT NULL,
	[OffsetMinutesFromFirstStation] SMALLINT DEFAULT 0, /* First station must have zero */
	[StopMinutes] TINYINT NOT NULL DEFAULT 0, /* Zero means no stop */
	CONSTRAINT [PK_LayoutRoutePatternLayoutStation] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_LayoutRoutePatternLayoutStation_LayoutRoutePattern] FOREIGN KEY ([LayoutRoutePatternId]) REFERENCES [dbo].[LayoutRoutePattern] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_LayoutRoutePatternLayoutStation_LayoutStation] FOREIGN KEY ([LayoutStationId]) REFERENCES [dbo].[LayoutStation] ([Id]),
)
