CREATE VIEW [dbo].[LayoutVehicles] AS 
	SELECT 
		[DccAddress] AS Id,
		[LayoutId],
		[DccAddress],
		[OperatorSignature],
		[VehicleClass],
		[VehicleNumber],
		[VehicleProviderName],
		[ThrottleIdentity],
		[IsTractionUnit]
	FROM [LayoutVehiclesImported] 
	
	UNION 

	SELECT 
		TVP.Id,
		T.LayoutId,
		TVP.DccAddress,
		VO.Signature AS OperatorSignature,
		COALESCE(TVP.OtherClass, TV.RequiredClass) AS VehicleClass,
		TVP.SingleVehicleNumber AS VehicleNumber,
		P.FirstName + ' ' + P.LastName AS VehicleProviderName,
		TVP.ThrottleIdentity,
		TV.IsTractionUnit
	FROM 
		Timetable T
		INNER JOIN TimetabledVehicle AS TV ON TV.TimetableId = T.Id
		INNER JOIN VehicleOperator AS VO ON TV.OperatorId = VO.Id
		INNER JOIN VehicleProvider TVP ON TVP.TimetableVehicleId = TV.Id
		INNER JOIN LayoutParticipant LP ON LP.Id = TVP.ProvidingLayoutParticipantId
		INNER JOIN Person P ON P.Id = LP.PersonId

