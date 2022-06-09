CREATE TABLE [dbo].[CargoPackagingUnit]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SingularResourceCode] NCHAR(50) NOT NULL, 
    [PluralResourceCode] NCHAR(50) NOT NULL, 
    [DisplayOrder] INT NOT NULL
)
