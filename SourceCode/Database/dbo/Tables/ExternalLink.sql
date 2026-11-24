CREATE TABLE [dbo].[ExternalLink] (
    [Id]                             INT                IDENTITY (1, 1) NOT NULL,
    [OwnerType]                      NVARCHAR (50)      NOT NULL,
    [OwnerId]                        INT                NOT NULL,
    [Purpose]                        TINYINT            NOT NULL,
    [Href]                           NVARCHAR (1000)    NOT NULL,
    [ContentType]                    NVARCHAR (100)     NOT NULL,
    [Language]                       NVARCHAR (3)       NULL,
    [IsDefault]                      BIT                NOT NULL,
    [FoundUnreachableDateTimeOffset] DATETIMEOFFSET (7) NULL,
    [CreatedAt]                      DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_ExternalLink] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ExternalLink_Language]
    ON [dbo].[ExternalLink]([OwnerType] ASC, [OwnerId] ASC, [Purpose] ASC, [Language] ASC);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ExternalLink_Default]
    ON [dbo].[ExternalLink]([OwnerType] ASC, [OwnerId] ASC, [Purpose] ASC) WHERE ([IsDefault] = 1);