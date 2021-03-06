SET IDENTITY_INSERT [dbo].[Country] ON 
GO

INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (1, N'Sweden', N'se', N'sv', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (2, N'Norway', N'no', N'no;nn', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (3, N'Denmark', N'dk', N'da', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (4, N'Germany', N'de', N'de', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (5, N'Switzerland', N'ch', N'de', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (6, N'United Kingdom', N'uk', N'en', NULL)
INSERT [dbo].[Country] ([Id], [EnglishName], [DomainSuffix], [Languages], [TimeZoneName]) VALUES (7, N'Netherlands', N'nl', N'nl', NULL)
GO

SET IDENTITY_INSERT [dbo].[Country] OFF
GO
