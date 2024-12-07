CREATE VIEW [dbo].[GroupCustomerCargo] AS
SELECT 
	G.Id AS GroupId,
	MON.Names AS OwnerNames,
	MS.MainTheme,
	SCC.Id,
	SCC.StationCustomerId,
	S.Id AS StationId,
	S.FullName AS StationName,
	S.[Signature] AS StationSignature,
	SCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	CAST (1 AS BIT) AS IsInternal,
	S.IsShadow AS IsShadowYard,
	C.DefaultClasses,
	G.CountryId,
	'#FFFFFF' AS BackColor,
	'#000000' AS ForeColor,
	SC.CustomerName,
	COALESCE(SCC.FromYear, SC.OpenedYear) AS FromYear,
	COALESCE(SCC.UptoYear, SC.ClosedYear) AS UptoYear,
	SCC.Quantity,
	SCC.QuantityUnitId,
	SCC.PackageUnitId,
	SCC.SpecificWagonClass,
	SCC.SpecialCargoName,
	CRT.ShortName AS ReadyTime,
	CRT.IsSpecifiedInLayout AS ReadyTimeIsSpecifiedInLayout,
	COALESCE(SCC.TrackOrArea, SC.TrackOrArea) AS TrackOrArea,
	CASE
		WHEN SCC.TrackOrAreaColor IS NOT NULL AND SCC.TrackOrAreaColor <> '#FFFFFF' THEN SCC.TrackOrAreaColor
		ELSE SC.TrackOrAreaColor
	END AS TrackOrAreaColor,
	SCC.EmptyReturn,
	SCC.MatchReturn,
	SCC.OperatingDayId
FROM 
	[Group] AS G INNER JOIN
	[GroupLayoutModule] AS GLM ON GLM.GroupId = G.Id INNER JOIN
	[Module] AS M ON M.Id = GLM.ModuleId INNER JOIN
	[ModuleStandard] AS MS ON MS.Id = M.StandardId INNER JOIN
	[Station] AS S ON S.PrimaryModuleId = M.Id INNER JOIN
	[ModuleOwnerNames] AS MON ON MON.ModuleId = M.Id INNER JOIN
	[StationCustomer] AS SC ON SC.StationId = S.Id INNER JOIN
	[StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id INNER JOIN
	[Cargo] AS C ON SCC.CargoId = C.Id INNER JOIN 
	[CargoDirection] AS CD ON SCC.DirectionId = CD.Id INNER JOIN 
	[CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId 
WHERE 
	M.FunctionalState > 3


