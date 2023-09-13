CREATE TABLE [dbo].[VehicleFeature]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
	[Category] VARCHAR(10) NOT NULL, 
	[OnlyScaleId] INT NULL,
	[Description] NVARCHAR(20) NOT NULL, 

	CONSTRAINT [PK_VehicleFeature] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_VehicleFeature_Scale] FOREIGN KEY ([OnlyScaleId]) REFERENCES [dbo].[Scale] ([Id]),
	CONSTRAINT [UX_VehicleFeatyre] UNIQUE ([Category], [Description], [OnlyScaleId])


)
