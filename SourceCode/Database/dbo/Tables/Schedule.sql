CREATE TABLE [dbo].[Schedule]
(
    [Id] INT NOT NULL IDENTITY (1, 1),
    [LayoutId] INT NOT NULL,
    [Number] SMALLINT NOT NULL, /* Identity of Schedule */
    [IsCargo] BIT NOT NULL DEFAULT 0, /* Because cargo schedules may not having a train unit attached */
    CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [AK_Schedule_Number] UNIQUE ([Number], [LayoutId]),
    CONSTRAINT [FK_Schedule_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [dbo].[Layout] ([Id]),
)
