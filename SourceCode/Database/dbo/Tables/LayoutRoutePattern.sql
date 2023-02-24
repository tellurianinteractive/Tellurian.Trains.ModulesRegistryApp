CREATE TABLE [dbo].[LayoutRoutePattern]
(
	[Id] INT NOT NULL IDENTITY (1, 1),
	[LayoutId] INT NOT NULL,
	[Description] NVARCHAR(10) NOT NULL,
	[TrainPreparationMinutes] TINYINT DEFAULT 0, /* Minutes before first departure when driver must be present */
	[TrainFinishingMinutes] TINYINT DEFAULT 0, /* Minutes after last arrival when driver must be present */
	CONSTRAINT [PK_LayoutRoutePattern] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_LayoutRoutePattern_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),

)
