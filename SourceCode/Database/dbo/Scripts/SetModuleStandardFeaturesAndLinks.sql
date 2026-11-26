-- One-off (2026-11-26): the accepted wheel profiles and couplings of the module standards (ModuleFeature) and their norm documents
-- (ExternalLink), with the values of the dropped columns ModuleStandard.Wheelset, Couplings and AcceptedNorm as they were read
-- from production earlier the same day. Vehicles refer to Feature instead of VehicleFeature (the same ids). Safe to run again.

-- Vehicles refer to Feature; the ids are the same.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vehicle_CouplingFeature' AND referenced_object_id = OBJECT_ID('dbo.VehicleFeature'))
    ALTER TABLE dbo.Vehicle DROP CONSTRAINT FK_Vehicle_CouplingFeature;
IF OBJECT_ID('dbo.FK_Vehicle_CouplingFeature') IS NULL
    ALTER TABLE dbo.Vehicle ADD CONSTRAINT FK_Vehicle_CouplingFeature FOREIGN KEY (CouplingFeatureId) REFERENCES dbo.Feature (Id);
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vehicle_WheelsFeature' AND referenced_object_id = OBJECT_ID('dbo.VehicleFeature'))
    ALTER TABLE dbo.Vehicle DROP CONSTRAINT FK_Vehicle_WheelsFeature;
IF OBJECT_ID('dbo.FK_Vehicle_WheelsFeature') IS NULL
    ALTER TABLE dbo.Vehicle ADD CONSTRAINT FK_Vehicle_WheelsFeature FOREIGN KEY (WheelsFeatureId) REFERENCES dbo.Feature (Id);
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vehicle_TractionFeature' AND referenced_object_id = OBJECT_ID('dbo.VehicleFeature'))
    ALTER TABLE dbo.Vehicle DROP CONSTRAINT FK_Vehicle_TractionFeature;
IF OBJECT_ID('dbo.FK_Vehicle_TractionFeature') IS NULL
    ALTER TABLE dbo.Vehicle ADD CONSTRAINT FK_Vehicle_TractionFeature FOREIGN KEY (TractionFeatureId) REFERENCES dbo.Feature (Id);
GO

-- Wheels: "NEM ... RP25" gave both; "CODE 110" and "RP25" RP25; "NEM" NEM310. AmericaN ("low-profile, 0.45 mm flange") is left.
-- Couplings: "FL 6511" FL6511, "Kadee" and "Kadee #713/#714" Kadee, "Micro-Trains". FREMO/N-RE ("Standard") is left.
WITH Accepted (ModuleStandardId, Category, Description) AS (
    SELECT * FROM (VALUES
        (1, 'WHEELS', N'NEM310'), (1, 'WHEELS', N'RP25'), (1, 'COUPLINGS', N'FL6511'),
        (2, 'WHEELS', N'RP25'), (2, 'COUPLINGS', N'FL6511'),
        (3, 'COUPLINGS', N'Micro-Trains'),
        (4, 'WHEELS', N'NEM310'), (4, 'WHEELS', N'RP25'), (4, 'COUPLINGS', N'FL6511'),
        (5, 'WHEELS', N'NEM310'),
        (6, 'WHEELS', N'NEM310'),
        (7, 'WHEELS', N'RP25'), (7, 'COUPLINGS', N'Kadee'),
        (8, 'COUPLINGS', N'Kadee'),
        (13, 'COUPLINGS', N'Kadee'),
        (16, 'WHEELS', N'RP25'), (16, 'COUPLINGS', N'FL6511'),
        (17, 'WHEELS', N'RP25'), (17, 'COUPLINGS', N'FL6511')
    ) AS V (ModuleStandardId, Category, Description)
)
INSERT dbo.ModuleFeature (ModuleStandardId, FeatureId)
SELECT A.ModuleStandardId, F.Id
FROM Accepted A
INNER JOIN dbo.ModuleStandard MS ON MS.Id = A.ModuleStandardId
INNER JOIN dbo.Feature F ON F.Category = A.Category AND F.Description = A.Description
WHERE NOT EXISTS (SELECT 1 FROM dbo.ModuleFeature MF WHERE MF.ModuleStandardId = A.ModuleStandardId AND MF.FeatureId = F.Id);
GO

-- Norm documents (purpose 1), the default of their purpose. FREMO/H0-RE had "link will follow", which is left.
WITH Norm (ModuleStandardId, Href, ContentType, Language) AS (
    SELECT * FROM (VALUES
        (1, N'https://www.malarmodulmote.se/wp-content/uploads/2019/04/M%C3%A4larModulM%C3%B6te-norm-2R-2.3.pdf', N'application/pdf', N'sv'),
        (7, N'http://www.smalsparigt.org/wp-content/uploads/2012/01/H0n3-modulnorm_0-92.pdf', N'application/pdf', N'sv'),
        (8, N'https://www.fremo-net.eu/de/modulsysteme/baugroesse-h0/h0-usa/hon3/hon3-norm/', N'text/html', N'de'),
        (9, N'https://www.fremo-net.eu/index.php?eID=tx_securedownloads&u=3302&g=1%2C6&t=1630830380&hash=eabfb99d04ad19d90c4800541c64118913059f98&file=fileadmin/modulsysteme/baugroesse_0/0_Regelspur/FREMO-Spur0-Norm_V10.pdf', N'application/pdf', NULL),
        (12, N'https://www.malarmodulmote.se/wp-content/uploads/2019/04/MMM-Norm-3R-Ver-2.0.pdf', N'application/pdf', N'sv'),
        (16, N'https://www.fremo-net.eu/en/modulsysteme/baugroesse-h0/h0-europa/industrie-und-hafen/der-ih05-modulkasten', N'text/html', N'en')
    ) AS V (ModuleStandardId, Href, ContentType, Language)
)
INSERT dbo.ExternalLink (OwnerType, OwnerId, Purpose, Href, ContentType, Language, IsDefault, CreatedAt)
SELECT N'ModuleStandard', N.ModuleStandardId, 1, N.Href, N.ContentType, N.Language, 1, SYSDATETIMEOFFSET()
FROM Norm N
INNER JOIN dbo.ModuleStandard MS ON MS.Id = N.ModuleStandardId
WHERE NOT EXISTS (SELECT 1 FROM dbo.ExternalLink L WHERE L.OwnerType = N'ModuleStandard' AND L.OwnerId = N.ModuleStandardId AND L.Purpose = 1);
GO

-- What was saved.
SELECT MS.Id, MS.ShortName, F.Category, F.Description
FROM dbo.ModuleFeature MF
INNER JOIN dbo.ModuleStandard MS ON MS.Id = MF.ModuleStandardId
INNER JOIN dbo.Feature F ON F.Id = MF.FeatureId
ORDER BY MS.Id, F.Category, F.Description;
SELECT OwnerId, Href, ContentType, Language, IsDefault FROM dbo.ExternalLink WHERE OwnerType = N'ModuleStandard' ORDER BY OwnerId;