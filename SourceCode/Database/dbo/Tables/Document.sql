CREATE TABLE [dbo].[Document] (
    [Id]               INT                IDENTITY (1, 1) NOT NULL,
    [FileExtension]    NCHAR (5)          NOT NULL,
    [ContentType]      NVARCHAR (50)      NULL,
    [Content]          VARBINARY (MAX)    NULL,
    [LastModifiedTime] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_Document] PRIMARY KEY CLUSTERED ([Id] ASC)
);

