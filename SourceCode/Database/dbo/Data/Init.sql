INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSupply) VALUES (1, 'Receive', 'In', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSupply) VALUES (2, 'Send', 'Out',1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSupply) VALUES (3, 'Import', 'Imp',1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSupply) VALUES (4, 'Export', 'Exp', 0)
GO

INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (0, 'NotApplicable', 'n/a', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (1, 'In the evening', 'Evening', 1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (2, 'At night', 'Night', 1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (4, 'In the afternoon', 'Afternoon', 1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (5, 'In the morning', 'Morning', 1)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (6, 'At 06:00', '06:00', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (7, 'At 09:00', '09:00', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (8, 'At 12:00', '12:00', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (9, 'At 15:00', '15:00', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (10, 'At 18:00', '18:00', 0)
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, ShortName, IsSpecifiedInLayout) VALUES (11, 'At 21:00', '21:00', 0)
GO

INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (1, 'Tonnes', 't')
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (2, 'Cubicmeters', 'm³')
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (3, 'Pieces', 'pcs')
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (4, 'Wagons', 'w')
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (5, 'Containers', 'con')
INSERT INTO [dbo].[CargoDirection]  (Id, FullName, Designation) VALUES (6, 'Trailers', 'tra')
GO

INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Sweden', 'se','sv')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Norway', 'no','no;nn')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Denmark', 'dk','da')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Germany', 'de','de')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Switzerland', 'ch','de')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('United Kingdom', 'uk','en')
INSERT INTO [dbo].[CargoDirection]  (EnglishName, DomainSuffix, Languages, TimeZoneName) VALUES ('Netherlands', 'nl','nl')
GO

INSERT INTO [dbo].[GroupDomain] ([Id], [Name]) VALUES (1, 'FREMO')
GO


