CREATE TABLE [dbo].[Operator] (
    [Id]                        INT           IDENTITY (1, 1) NOT NULL,
    [Signature]                 NVARCHAR (6)  NOT NULL,
    [FullName]                  NVARCHAR (50) NOT NULL,
    [PrimaryOperatingCountryId] INT           NOT NULL,
    [OperatingFromYear]         SMALLINT      NULL,
    [OperatingUptoYear]         SMALLINT      NULL,
    [IsPassengerOperator]       BIT           CONSTRAINT [DF_Operator_IsPassengerOperator] DEFAULT ((0)) NOT NULL,
    [IsFreightOperator]         BIT           CONSTRAINT [DF_Operator_IsFreightOperator] DEFAULT ((0)) NOT NULL,
    [IsConstructionOperator]    BIT           CONSTRAINT [DF_Operator_IsConstructionOperator] DEFAULT ((0)) NOT NULL,
    [IsVeteranOperator]         BIT           CONSTRAINT [DF_Operator_IsVeteranOperator] DEFAULT ((0)) NOT NULL,
    [IsAuthority]               BIT           CONSTRAINT [DF_Operator_IsAuthority] DEFAULT ((0)) NOT NULL,
    [IsPrivate]                 BIT           CONSTRAINT [DF_Operator_IsPrivate] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Operator] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE INDEX [IX_Operator_Unique] ON [dbo].[Operator] ([Signature], [PrimaryOperatingCountryId])
