CREATE TABLE [dbo].[VehicleOperator] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [Signature]                 NVARCHAR (6)   NOT NULL,
    [ShortName]                 NVARCHAR (30)  NOT NULL,
    [FullName]                  NVARCHAR (100) NOT NULL,
    [RICS]                      SMALLINT       NULL,
    [LogotypeImage]             VARBINARY(MAX) NULL,
    [PrimaryOperatingCountryId] INT            NOT NULL,
    [FirstYearInOperation]      SMALLINT       NULL,
    [FinalYearInOperation]      SMALLINT       NULL,
    [IsPassengerOperator]       BIT            DEFAULT ((0)) NOT NULL,
    [IsFreightOperator]         BIT            DEFAULT ((0)) NOT NULL,
    [IsConstructionOperator]    BIT            DEFAULT ((0)) NOT NULL,
    [IsVeteranOperator]         BIT            DEFAULT ((0)) NOT NULL,
    [OnlyInLayoutId]            INT            NULL,

    CONSTRAINT [PK_Operator] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Operator_PrimaryOperatingCountry] FOREIGN KEY ([PrimaryOperatingCountryId]) REFERENCES [dbo].[Country] ([Id]),
    CONSTRAINT [FK_Operator_OnlyInLayout] FOREIGN KEY ([OnlyInLayoutId]) REFERENCES [dbo].[Layout] ([Id]),
    CONSTRAINT [UX_VehicleOperator] UNIQUE ([PrimaryOperatingCountryId], [Signature], [FirstYearInOperation], [OnlyInLayoutId])
);

GO

