CREATE TABLE [dbo].[CargoUnit] (
	[Id]                    INT NOT NULL,
	[SingularResourceCode]  NVARCHAR(50) NOT NULL, 
	[PluralResourceCode]    NVARCHAR(50) NOT NULL, 
	[DisplayOrder]          INT NOT NULL DEFAULT(0),
	[FullName]              NVARCHAR (12) NOT NULL,
	[Designation]           NVARCHAR (6)  NOT NULL,
	[IsBearer]              BIT DEFAULT(0),
	CONSTRAINT [PK_CargoUnit] PRIMARY KEY CLUSTERED ([Id] ASC)
);

