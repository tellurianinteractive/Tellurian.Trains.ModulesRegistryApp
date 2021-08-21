-- Not implemented. Waybills are implemeted using a view.

CREATE TABLE [dbo].[CargoRelationWagon] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [CargoRelationId] INT           NOT NULL,
    [WagonNumber]     INT           NOT NULL,
    [IsEmpty]         BIT           CONSTRAINT [DF_CargoRelationWagon_IsEmpty] DEFAULT ((0)) NOT NULL,
    [HasReturn]       BIT           CONSTRAINT [DF_CargoRelationWagon_HasReturn] DEFAULT ((0)) NOT NULL,
    [OperatorId]      INT           NULL,
    [WagonClassId]    INT           NULL,
    [Note]            NVARCHAR (50) NULL,
    CONSTRAINT [PK_CargoRelationWagon] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CargoRelationWagon_CargoRelation] FOREIGN KEY ([CargoRelationId]) REFERENCES [dbo].[CargoRelation] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CargoRelationWagon_Operator] FOREIGN KEY ([OperatorId]) REFERENCES [dbo].[Operator] ([Id])
);

