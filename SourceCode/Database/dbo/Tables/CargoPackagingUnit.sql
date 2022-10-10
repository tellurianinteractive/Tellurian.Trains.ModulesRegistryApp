CREATE TABLE [dbo].[CargoPackagingUnit]
(
	[Id] INT NOT NULL PRIMARY KEY, 
	[SingularResourceCode] NVARCHAR(50) NOT NULL, 
	[PluralResourceCode] NVARCHAR(50) NOT NULL, 
	[DisplayOrder] INT NOT NULL, 
    [PrepositionResourceCode] NVARCHAR(4) NULL
)
