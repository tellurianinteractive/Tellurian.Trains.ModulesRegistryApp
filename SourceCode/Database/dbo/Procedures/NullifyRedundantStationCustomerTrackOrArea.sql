CREATE PROCEDURE [dbo].[NullifyRedundantStationCustomerTrackOrArea]
AS
	UPDATE 
		StationCustomerCargo 
	SET 
		TrackOrArea = NULL WHERE Id IN 
		(
			SELECT 
				SCC.Id 
			FROM 
				StationCustomerCargo SCC INNER JOIN 
				StationCustomer SC ON SCC.StationCustomerId = SC.Id 
			WHERE 
				SC.TrackOrArea = SCC.TrackOrArea
		)

	UPDATE 
		StationCustomerCargo 
	SET 
		TrackOrAreaColor = NULL WHERE Id IN 
		(
			SELECT 
				SCC.Id 
			FROM 
				StationCustomerCargo SCC INNER JOIN 
				StationCustomer SC ON SCC.StationCustomerId = SC.Id 
			WHERE 
				SC.TrackOrAreaColor = SCC.TrackOrAreaColor
		)
RETURN 0
