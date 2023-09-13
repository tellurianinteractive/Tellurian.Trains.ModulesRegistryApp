CREATE TABLE [dbo].[MeetingPurchaseableItem]
(
    [Id] INT IDENTITY (1, 1) NOT NULL, 
    [MeetingId] INT NOT NULL, 
    [Category] INT NOT NULL, 
    [Description] NVARCHAR(50),
    [NumberAvailable] INT NOT NULL DEFAULT 1, 
    [ItemPriceEuro] MONEY NOT NULL, 
    [ItemPriceLocal] MONEY NULL, 
    [IsMandatory] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_MeetingPurchaseableItem] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingPurchaseableItem_Meeting] FOREIGN KEY ([MeetingId]) REFERENCES [dbo].[Meeting] ([Id]) ON DELETE CASCADE,
)
