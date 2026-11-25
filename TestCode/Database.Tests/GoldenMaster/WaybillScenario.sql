-- The test world for the waybill golden-master tests (Migration step 5.1, System Design Documents/Waybills.md).
-- Explicit ids, so the results don't depend on identity values (they are not rolled back with the test's transaction).
-- Id ranges are kept apart (module stations 1-9, external stations 31-39, module customers 11-29, external customers 41-49,
-- module flows 101-199, external flows 201-299), so the procedures' comparisons across id spaces don't meet by chance.
-- The lookup tables CargoDirection, CargoReadyTime and OperatingDay come from the SQL project's initial data scripts.

INSERT INTO CargoUnit (Id, SingularResourceCode, PluralResourceCode, FullName, Designation, DisplayOrder, IsBearer) VALUES
    (1, 'Ton', 'Tons', 'Ton', 't', 1, 0),
    (3, 'Piece', 'Pieces', 'Piece', '', 3, 0),
    (4, 'Wagon', 'Wagons', 'Wagon', '', 4, 1);

INSERT INTO CargoPackagingUnit (Id, SingularResourceCode, PluralResourceCode, DisplayOrder, PrepositionResourceCode) VALUES
    (0, 'None', 'None', 0, NULL),
    (1, 'Pallet', 'Pallets', 1, 'On');

INSERT INTO NHM (Id, Code, LevelDigits, HasEmptyWagonOrder, DA, DE, EN, NL, PL, SV) VALUES
    (44000000, '44000000', 2, 1, 'Træ', 'Holz', 'Wood', 'Hout', 'Drewno', 'Trä'),
    (10000000, '10000000', 2, 0, 'Korn', 'Getreide', 'Cereals', 'Granen', 'Zboża', 'Spannmål'),
    (27000000, '27000000', 2, 0, 'Olie', 'Öl', 'Oil', 'Olie', 'Olej', 'Olja');

SET IDENTITY_INSERT Cargo ON;
INSERT INTO Cargo (Id, NHMCode, DefaultClasses, FromYear, UptoYear, EN, SV, IsExpress, IsCoolingRequired) VALUES
    (1, 44000000, 'Ks', NULL, NULL, 'Timber', 'Timmer', 0, 0),
    (2, 10000000, 'Gbs', 1900, 1960, 'Grain', 'Spannmål', 0, 0),
    (3, 0, 'G', NULL, NULL, 'General cargo', 'Styckegods', 0, 0),
    (4, 27000000, 'Zaes', NULL, NULL, 'Oil', 'Olja', 0, 0);
SET IDENTITY_INSERT Cargo OFF;

SET IDENTITY_INSERT Country ON;
INSERT INTO Country (Id, EnglishName, IsoCode, DomainSuffix, Languages, PhoneNumber) VALUES
    (1, 'Sweden', 'SE', 'se', 'sv', 46),
    (2, 'Norway', 'NO', 'no', 'nb', 47);
SET IDENTITY_INSERT Country OFF;

SET IDENTITY_INSERT Region ON;
INSERT INTO Region (Id, CountryId, LocalName, ForeColor, BackColor) VALUES
    (1, 1, 'Norrland', '#FFFFFF', '#0000FF'),  -- Representative external station: Umeå.
    (2, 1, 'Skåne', '#000000', '#FFFF00'),     -- No representative external station.
    (3, 2, 'Østlandet', '#FFFFFF', '#FF0000'); -- Representative external station: Oslo.
SET IDENTITY_INSERT Region OFF;

SET IDENTITY_INSERT ExternalStation ON;
INSERT INTO ExternalStation (Id, RegionId, FullName, Signature, InternationalName) VALUES
    (31, 1, 'Umeå', 'Umå', NULL),
    (32, 2, 'Malmö', 'M', 'Malmoe'),
    (33, 3, 'Oslo', 'Osl', NULL);
SET IDENTITY_INSERT ExternalStation OFF;

UPDATE Region SET RepresentativeExternalStationId = 31 WHERE Id = 1;
UPDATE Region SET RepresentativeExternalStationId = 33 WHERE Id = 3;

SET IDENTITY_INSERT Scale ON;
INSERT INTO Scale (Id, ShortName, Denominator) VALUES (1, 'H0', 87), (2, 'N', 160);
SET IDENTITY_INSERT Scale OFF;

SET IDENTITY_INSERT ModuleStandard ON;
INSERT INTO ModuleStandard (Id, ShortName, ScaleId, MainThemeId) VALUES
    (1, 'H0 Europe', 1, 1),
    (2, 'H0 USA', 1, 2),
    (3, 'N Europe', 2, 1);
SET IDENTITY_INSERT ModuleStandard OFF;

SET IDENTITY_INSERT Person ON;
INSERT INTO Person (Id, FirstName, LastName, CountryId) VALUES
    (1, 'Anna', 'Andersson', 1),
    (2, 'Bjørn', 'Berg', 2),
    (3, 'Cecilia', 'Carlsson', 1);
SET IDENTITY_INSERT Person OFF;

SET IDENTITY_INSERT [Group] ON;
INSERT INTO [Group] (Id, CountryId, FullName, ShortName, Category) VALUES (1, 1, 'Modulgruppen', 'MG', 'Club');
SET IDENTITY_INSERT [Group] OFF;

SET IDENTITY_INSERT Station ON;
INSERT INTO Station (Id, FullName, Signature, IsShadow, IsTerminus, RegionId) VALUES
    (1, 'Alby', 'Ab', 0, 0, 1),
    (2, 'Berga', 'Bg', 0, 0, 1),
    (3, 'Cederby', 'Cb', 0, 0, 2),
    (4, 'Dalen', 'Dl', 0, 0, 3),
    (5, 'Eastville', 'Ev', 0, 0, 1),   -- USA main theme.
    (6, 'Fjärdingen', 'Fj', 0, 0, 2);  -- N scale.
SET IDENTITY_INSERT Station OFF;

SET IDENTITY_INSERT Module ON;
-- The themes are the standards' themes, as SetModuleAndLayoutThemes.sql gives them.
INSERT INTO Module (Id, FullName, ScaleId, StandardId, Length, StationId, FunctionalState, PrimaryThemeId) VALUES
    (1, 'Alby', 1, 1, 1.5, 1, 4, 1),
    (2, 'Berga', 1, 1, 1.5, 2, 4, 1),
    (3, 'Cederby', 1, 1, 1.5, 3, 2, 1),  -- Not functional: left out of group waybills.
    (4, 'Dalen', 1, 1, 1.5, 4, 4, 1),
    (5, 'Eastville', 1, 2, 1.5, 5, 4, 2),
    (6, 'Fjärdingen', 2, 3, 1.0, 6, 4, 1);
SET IDENTITY_INSERT Module OFF;

UPDATE Station SET PrimaryModuleId = Id;

-- One row per statement: the trigger AddModuleToGroupLayoutModules fails on an insert of several rows.
-- It also adds the group-owned module (Eastville) to the group's layout modules.
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (1, 1, NULL, 1);
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (2, 1, NULL, 1);
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (3, 3, NULL, 1);
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (4, 2, NULL, 1);
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (5, NULL, 1, 1);
INSERT INTO ModuleOwnership (ModuleId, PersonId, GroupId, OwnedShare) VALUES (6, 3, NULL, 1);

SET IDENTITY_INSERT StationCustomer ON;
INSERT INTO StationCustomer (Id, StationId, CustomerName, OpenedYear, ClosedYear) VALUES
    (11, 1, 'Alby sågverk', NULL, NULL),
    (12, 1, 'Alby kvarn', 1950, 1990),
    (13, 2, 'Berga skog', NULL, NULL),
    (14, 3, 'Cederby export', NULL, NULL),
    (15, 3, 'Cederby lantmän', NULL, NULL),
    (16, 4, 'Dalen bruk', NULL, NULL),
    (17, 5, 'Eastville lumber', NULL, NULL),
    (18, 6, 'Fjärdingen trä', NULL, NULL),
    (19, 2, 'Berga kvarn', 1965, 1990);
SET IDENTITY_INSERT StationCustomer OFF;

-- DirectionId: 1 Receive, 2 Send, 3 Import, 4 Export. QuantityUnitId: 1 ton, 3 piece, 4 wagon (bearer).
-- OperatingDayId: 1 Monday, 8 Daily, 10 Monday-Friday. ReadyTimeId: 1 evening, 4 morning.
SET IDENTITY_INSERT StationCustomerCargo ON;
INSERT INTO StationCustomerCargo (Id, StationCustomerId, CargoId, DirectionId, QuantityUnitId, Quantity, PackageUnitId, OperatingDayId, ReadyTimeId, FromYear, UptoYear, TrackOrArea) VALUES
    (101, 11, 1, 1, 3, 2, 0, 8, 4, NULL, NULL, 'Spår 3'),     -- Alby receives timber (2 pieces).
    (102, 11, 3, 1, 1, 5, 1, 10, 4, NULL, NULL, NULL),        -- Alby receives general cargo (no NHM code).
    (103, 11, 4, 3, 4, 2, 0, 8, 1, NULL, NULL, NULL),         -- Alby imports oil (2 wagons).
    (104, 12, 2, 1, 1, 20, 0, 8, 4, NULL, NULL, NULL),        -- Alby kvarn receives grain; customer years 1950-1990.
    (105, 13, 1, 2, 4, 3, 0, 1, 1, NULL, NULL, NULL),         -- Berga sends timber (3 wagons), Mondays.
    (106, 13, 3, 2, 1, 2, 1, 8, 1, NULL, NULL, NULL),         -- Berga sends general cargo.
    (107, 14, 1, 4, 4, 1, 0, 8, 1, NULL, NULL, NULL),         -- Cederby exports timber: also matches Alby's receive in the same country.
    (108, 14, 3, 1, 1, 1, 1, 8, 4, NULL, NULL, NULL),         -- Cederby receives general cargo; its region has no representative station.
    (109, 15, 2, 2, 1, 10, 0, 8, 1, 1900, 1940, NULL),        -- Cederby sends grain 1900-1940: no overlap with Alby kvarn.
    (110, 15, 2, 2, 1, 10, 0, 8, 1, 1930, 1960, NULL),        -- Cederby sends grain 1930-1960: overlaps Alby kvarn.
    (111, 16, 4, 4, 4, 2, 0, 8, 1, NULL, NULL, NULL),         -- Dalen (Norway) exports oil: matches Alby's import.
    (112, 16, 1, 2, 4, 1, 0, 8, 1, NULL, NULL, NULL),         -- Dalen (Norway) sends timber: other country, not international.
    (113, 17, 1, 2, 4, 1, 0, 8, 1, NULL, NULL, NULL),         -- Eastville sends timber: other main theme.
    (114, 18, 1, 2, 4, 1, 0, 8, 1, NULL, NULL, NULL),         -- Fjärdingen sends timber: other scale.
    (115, 13, 4, 2, 4, 4, 0, 8, 1, NULL, NULL, NULL),         -- Berga sends oil (empty wagon order: no, oil's NHM has none).
    (116, 19, 2, 1, 1, 20, 0, 8, 4, NULL, NULL, NULL),        -- Berga kvarn receives grain; customer years 1965-1990.
    (117, 15, 2, 2, 1, 10, 0, 8, 1, 1940, 1970, NULL);        -- Cederby sends grain 1940-1970: the window with Berga kvarn is 1965-1970, after grain (up to 1960).
SET IDENTITY_INSERT StationCustomerCargo OFF;

SET IDENTITY_INSERT ExternalStationCustomer ON;
INSERT INTO ExternalStationCustomer (Id, ExternalStationId, CustomerName) VALUES
    (41, 32, 'Malmö hamn'),
    (42, 33, 'Oslo havn'),
    (43, 31, 'Umeå energi');
SET IDENTITY_INSERT ExternalStationCustomer OFF;

SET IDENTITY_INSERT ExternalStationCustomerCargo ON;
INSERT INTO ExternalStationCustomerCargo (Id, ExternalStationCustomerId, CargoId, DirectionId, QuantityUnitId, Quantity, PackageUnitId, OperatingDayId) VALUES
    (201, 41, 1, 2, 4, 4, 0, 8),   -- Malmö sends timber (4 wagons): matches Alby's receive.
    (202, 42, 1, 3, 4, 2, 0, 8),   -- Oslo imports timber: matches Cederby's export.
    (203, 43, 4, 1, 4, 1, 0, 10),  -- Umeå receives oil: matches Berga's send.
    (204, 41, 3, 1, 1, 1, 1, 8);   -- Malmö receives general cargo: no NHM code, so never matched with module stations.
SET IDENTITY_INSERT ExternalStationCustomerCargo OFF;

-- A meeting with two similar layouts (H0 Europe) and one other (H0 USA).
SET IDENTITY_INSERT Meeting ON;
INSERT INTO Meeting (Id, OrganiserGroupId, CityName, Name, StartDate, EndDate, Status) VALUES
    (1, 1, 'Alby', 'Albyträffen', '2026-05-01', '2026-05-03', 2);
SET IDENTITY_INSERT Meeting OFF;

SET IDENTITY_INSERT Layout ON;
INSERT INTO Layout (Id, MeetingId, OrganisingGroupId, PrimaryModuleStandardId, RegistrationOpeningDate, RegistrationClosingDate, FirstYear, LastYear, ThemeId) VALUES
    (1, 1, 1, 1, '2026-01-01', '2026-04-01', 1940, 1970, 1),
    (2, 1, 1, 1, '2026-01-01', '2026-04-01', NULL, NULL, 1),
    (3, 1, 1, 2, '2026-01-01', '2026-04-01', NULL, NULL, 2);
SET IDENTITY_INSERT Layout OFF;

SET IDENTITY_INSERT MeetingParticipant ON;
INSERT INTO MeetingParticipant (Id, PersonId, MeetingId, RegistrationTime, CancellationTime) VALUES
    (1, 1, 1, '2026-02-01T10:00:00+01:00', NULL),
    (2, 2, 1, '2026-02-01T10:00:00+01:00', NULL),
    (3, 3, 1, '2026-02-01T10:00:00+01:00', '2026-03-01T10:00:00+01:00'); -- Cancelled.
SET IDENTITY_INSERT MeetingParticipant OFF;

SET IDENTITY_INSERT LayoutParticipant ON;
INSERT INTO LayoutParticipant (Id, MeetingParticipantId, LayoutId, PersonId) VALUES
    (1, 1, 1, 1),
    (2, 2, 2, 2),
    (3, 2, 3, 2),
    (4, 3, 1, 3);
SET IDENTITY_INSERT LayoutParticipant OFF;

INSERT INTO LayoutModule (LayoutParticipantId, ModuleId, RegistrationStatus) VALUES (1, 1, 2), (1, 2, 2), (2, 4, 2), (3, 5, 2), (4, 3, 2);
INSERT INTO LayoutStation (LayoutParticipantId, StationId) VALUES
    (1, 1), (1, 2),  -- Layout 1: Alby, Berga.
    (2, 4),          -- Layout 2: Dalen.
    (3, 5),          -- Layout 3: Eastville.
    (4, 3);          -- Layout 1: Cederby, but its participant has cancelled.

INSERT INTO GroupLayoutModule (GroupId, ModuleId) VALUES (1, 1), (1, 2), (1, 3), (1, 4);