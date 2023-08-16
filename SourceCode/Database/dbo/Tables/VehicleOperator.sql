CREATE TABLE [dbo].[VehicleOperator] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [Signature]                 NVARCHAR (6)   NOT NULL,
    [FullName]                  NVARCHAR (50)  NOT NULL,
    [LogotypeImage]             VARBINARY(MAX) NULL,
    [PrimaryOperatingCountryId] INT            NOT NULL,
    [FirstYearInOperation]      SMALLINT       NULL,
    [FinalYearInOperation]      SMALLINT       NULL,
    [IsPassengerOperator]       BIT            DEFAULT ((0)) NOT NULL,
    [IsFreightOperator]         BIT            DEFAULT ((0)) NOT NULL,
    [IsConstructionOperator]    BIT            DEFAULT ((0)) NOT NULL,
    [IsVeteranOperator]         BIT            DEFAULT ((0)) NOT NULL,
    [IsAuthority]               BIT            DEFAULT ((0)) NOT NULL,
    [IsPrivate]                 BIT            DEFAULT ((0)) NOT NULL,
    [IsFictive]                 BIT            DEFAULT ((0)) NOT NULL, 
    CONSTRAINT [PK_Operator] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Operator_PrimaryOperatingCountry] FOREIGN KEY ([PrimaryOperatingCountryId]) REFERENCES [dbo].[Country] ([Id]),
);

GO

CREATE UNIQUE INDEX [IX_VehicleOperator_Unique] ON [dbo].[VehicleOperator] ([Signature], [PrimaryOperatingCountryId])
