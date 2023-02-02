CREATE VIEW [dbo].[LayoutCustomerCargo] AS 	
SELECT -- Internal station customer cargo
	LP.LayoutId,
	SCC.Id,
	SCC.StationCustomerId,
	SCC.OperatingDayId,
	S.[Id] AS StationId,
	COALESCE(LS.OtherName, S.FullName) AS StationName,
	COALESCE(LS.OtherSignature, S.Signature) AS StationSignature,
	SCC.CargoId,
	CD.IsSupply,
	CD.IsInternational,
	CAST (1 AS BIT) AS IsInternal,
	CAST (0 AS BIT) AS IsShadowYard,
	C.DefaultClasses,
	COALESCE(LS.OtherCountryId, R.CountryId) AS CountryId,
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
	SCC.MatchReturn
FROM 
	[Station] AS S INNER JOIN 
	[Region] AS R ON S.RegionId = R.Id INNER JOIN 
	[StationCustomer] AS SC ON SC.StationId = S.Id INNER JOIN 
	[StationCustomerCargo] AS SCC ON SCC.StationCustomerId = SC.Id INNER JOIN 
	[Cargo] AS C ON SCC.CargoId = C.Id INNER JOIN 
	[CargoDirection] AS CD ON SCC.DirectionId = CD.Id INNER JOIN 
	[CargoReadyTime] AS CRT ON CRT.Id = SCC.ReadyTimeId INNER JOIN 
	[LayoutStation] AS LS ON LS.StationId = S.Id INNER JOIN 
	[LayoutParticipant] LP ON LS.LayoutParticipantId = LP.Id INNER JOIN
	[MeetingParticipant] MP ON LP.MeetingParticipantId = MP.Id
WHERE
	MP.CancellationTime IS NULL

