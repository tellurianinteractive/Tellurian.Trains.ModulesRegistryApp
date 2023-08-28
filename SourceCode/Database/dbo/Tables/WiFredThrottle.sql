CREATE TABLE [dbo].[WiFredThrottle]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [MacAddress] CHAR(17) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Configuration] VARCHAR(MAX) NULL,
    [RegistrationDateTime] DATETIMEOFFSET NOT NULL,
    [ValidationDateTime] DATETIMEOFFSET NULL,
    [UpdatedDateTime] DATETIMEOFFSET NULL,
    [DeletedDateTime] DATETIMEOFFSET NULL,
    [OwningPersonId] INT NOT NULL,
    [InventoryNumber] SMALLINT NOT NULL,
    [LocoAddress1] SMALLINT NULL,
    [LocoAddress2] SMALLINT NULL,
    [LocoAddress3] SMALLINT NULL,
    [LocoAddress4] SMALLINT NULL,

    CONSTRAINT [PK_WiFredThrottle] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Person] FOREIGN KEY ([OwningPersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [UX_WIFredThrottle_InventoryNumber] UNIQUE (OwningPersonId, InventoryNumber),
    CONSTRAINT [UX_WIFredThrottle_MacAddress] UNIQUE (MacAddress),

)
