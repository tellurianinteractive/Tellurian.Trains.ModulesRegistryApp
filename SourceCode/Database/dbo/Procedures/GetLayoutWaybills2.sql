﻿-- Fetches waybills across all layouts that has the same theme and scale as the layout given as parameter.
CREATE PROCEDURE [dbo].[GetLayoutWaybills2]
	@LayoutId INT,
	@StationId INT = NULL
AS
	SET NOCOUNT ON;

	DECLARE @MeetingId INT, @MainThemeId INT, @ScaleId INT 

	SELECT TOP 1 
		@MeetingId = L.MeetingId, 
		@MainThemeId = MS.MainThemeId, 
		@ScaleId = MS.ScaleId
	FROM 
		Layout AS L INNER JOIN 
		LayoutParticipant AS LP ON LP.LayoutId = L.Id INNER JOIN 
		LayoutModule AS LM ON LM.LayoutParticipantId = LP.Id  INNER JOIN 
		Module AS M ON M.Id = LM.ModuleId INNER JOIN 
		ModuleStandard AS MS ON MS.Id = PrimaryModuleStandardId 
	WHERE L.Id = @LayoutId

	SET NOCOUNT ON;
	SELECT
		CCC.Id,
		CCS.StationId AS OriginStationId,
		CCS.StationCustomerId AS OriginStationCustomerId,
		CCS.StationName AS OriginStationName,
		NULL AS OriginInternationalStationName,
		CCS.CustomerName AS SenderName,
		CCS.TrackOrArea AS SenderTrackOrArea,
		CCS.TrackOrAreaColor AS SenderTrackOrAreaColor,
		CCS.TrackOrArea AS SenderCargoTrackOrArea,
		CCS.TrackOrAreaColor AS SenderCargoTrackOrAreaColor,
		SC.Languages AS OriginLanguages,
		SC.DomainSuffix AS OriginDomainSuffix,
		CCS.BackColor AS OriginBackColor,
		CCS.ForeColor AS OriginForeColor,
		CCS.IsInternal AS OriginIsInternal,
		CCS.ReadyTime AS SenderReadyTime,
		ODS.Flag AS SendingDayFlag,
		CAST(1 AS BIT) AS OriginIsModuleStation,
		CCS.FromYear AS SenderFromYear,
		CCS.UptoYear AS SenderUptoYear,
		CAST(0 AS BIT) OriginInExternal,
		CCC.StationId AS DestinationStationId,
		CCC.StationCustomerId AS DestinationStationCustomerId,
		CCC.StationName AS DestinationStationName,
		NULL AS DestinationInternationalStationName,
		CCC.CustomerName AS ReceiverName,
		CCC.TrackOrArea AS ReceiverTrackOrArea,
		CCC.TrackOrAreaColor AS ReceiverTrackOrAreaColor,
		CCC.TrackOrArea AS ReceiverCargoTrackOrArea,
		CCC.TrackOrAreaColor AS ReceiverCargoTrackOrAreaColor,
		CC.Languages AS DestinationLanguages,
		CC.DomainSuffix AS DestinationDomainSuffix,
		CCC.BackColor AS DestinationBackColor,
		CCC.ForeColor AS DestinationForeColor,
		CCC.IsInternal AS DestinationIsInternal,
		CCC.ReadyTime AS ReceiverReadyTime,
		ODC.Flag AS ReceivingDayFlag,
		CAST(1 AS BIT) AS DestinationIsModuleStation,
		CCC.SpecialCargoName,
		C.DefaultClasses,
		CCC.SpecificWagonClass,
		CU.IsBearer AS QuantityIsBearer,
		CCC.QuantityUnitId,
		CCC.Quantity,
		CASE
			WHEN CCC.Quantity = 1 THEN CU.SingularResourceCode
			ELSE CU.PluralResourceCode
		END AS QuanityUnitResourceName,
		CU.Designation AS QuantityShortUnit,
		CPU.PluralResourceCode AS PackagingUnitResourceName,
		CPU.PrepositionResourceCode AS PackagingPrepositionResourceCode,
		CCC.PackageUnitId AS DestinationPackageUnitId, 
		CCC.FromYear AS ReceiverFromYear,
		CCC.UptoYear AS ReceiverUptoYear,
		CAST (0 AS BIT) AS DestinationIsExternal,
		C.DefaultClasses,
		C.NHMCode,
		C.DA,
		C.DE,
		C.EN,
		C.FR,
		C.IT,
		C.NL,
		C.NB,
		C.PL,
		C.SV,
		C.IsExpress,
		C.IsCoolingRequired,
		CASE
			WHEN CU.IsBearer <> 0 THEN CCC.Quantity
			ELSE 1
		END AS PrintCount,
		CAST (0 AS BIT) AS PrintPerOperatingDay,
		CAST (0 AS BIT) AS HasEmptyReturn,
		CAST (0 AS BIT) AS HasSameCargoReturn,
		MON.Names AS OwnerNames
	FROM
		LayoutCustomerCargo AS CCS INNER JOIN
		LayoutCustomerCargo AS CCC ON CCC.CargoId = CCS.CargoId INNER JOIN
		Cargo AS C ON C.Id = CCS.CargoId INNER JOIN
		OperatingDay AS ODS ON ODS.Id = CCS.OperatingDayId INNER JOIN
		OperatingDay AS ODC ON ODC.Id = CCC.OperatingDayId INNER JOIN
		Country AS SC ON SC.Id = CCS.CountryId INNER JOIN
		Country AS CC ON CC.Id = CCC.CountryId INNER JOIN
		CargoUnit AS CU ON CU.Id = CCC.QuantityUnitId INNER JOIN
		CargoPackagingUnit AS CPU ON CPU.Id = CCC.PackageUnitId INNER JOIN
		ModuleOwnerNames AS MON ON MON.StationId = CCC.StationId INNER JOIN
		Layout AS L ON L.Id = CCC.LayoutId
	WHERE
		CCS.CargoId = CCC.CargoId AND
		ISNULL(C.NHMCode,0) > 0 AND
		(CCS.FromYear IS NULL OR CCS.FromYear <= ISNULL(L.LastYear, 9999)) AND
		(CCS.UptoYear IS NULL OR CCS.UptoYear >= ISNULL(L.FirstYear, 0)) AND
		(CCC.FromYear IS NULL OR CCC.FromYear <= ISNULL(L.LastYear, 9999)) AND
		(CCC.UptoYear IS NULL OR CCC.UptoYear >= ISNULL(L.FirstYear, 0)) AND
		CCS.IsSupply <> 0 AND CCC.IsSupply = 0 AND
		CCS.StationId <> CCC.StationId AND 
		CCS.LayoutId IN ( SELECT Id FROM GetSimilarLayoutsAtMeeting(@MeetingId, @MainThemeId, @ScaleId) ) AND 
		CCC.LayoutId = @LayoutId AND
		(@StationId IS NULL OR CCC.StationId = @StationId)
	ORDER BY
		CCC.StationName, 
		CCC.CustomerName,
		CCS.StationName,
		CCS.CustomerName

RETURN 0
