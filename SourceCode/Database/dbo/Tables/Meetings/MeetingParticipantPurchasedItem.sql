﻿CREATE TABLE [dbo].[MeetingParticipantPurchasedItem]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[MeetingParticipantId] INT NOT NULL,
	[MeetingPurchaseableItemId] INT NOT NULL,
	[InvoiveNumber] INT NULL,
	[InvoiceDateTime] DATETIMEOFFSET NULL,
	[PaymentDateTime] DATETIMEOFFSET NULL,
    CONSTRAINT [PK_MeetingParticipantPurchasedItem] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetingParticipantPurchaseableItem_MeetingParticipant] FOREIGN KEY ([MeetingParticipantId]) REFERENCES [dbo].[MeetingParticipant] ([Id]),
    CONSTRAINT [FK_MeetingParticipantPurchasedItem_MeetingPurchaseableItem] FOREIGN KEY ([MeetingPurchaseableItemId]) REFERENCES [dbo].[MeetingPurchaseableItem] ([Id]) ON DELETE CASCADE,
)
