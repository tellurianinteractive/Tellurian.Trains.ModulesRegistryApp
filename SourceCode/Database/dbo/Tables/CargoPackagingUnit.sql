CREATE TABLE [dbo].[CargoPackagingUnit]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SingularResourceCode] NCHAR(10) NOT NULL, 
    [PluralResourceCode] NCHAR(10) NOT NULL, 
    [DisplayOrder] INT NOT NULL
)
