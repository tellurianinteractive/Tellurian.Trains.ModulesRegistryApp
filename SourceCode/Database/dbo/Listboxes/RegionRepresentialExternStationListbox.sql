CREATE VIEW [dbo].[RegionRepresentativeExternStationListbox]
	AS 
	SELECT        
		R.Id, 
		ES.FullName AS StationName, 
		R.LocalName AS RegionName,
		R.CountryId,
		R.ForeColor,
		R.BackColor
	FROM          
		Country AS C INNER JOIN 
		Region AS R ON R.CountryId = C.Id LEFT OUTER JOIN
		ExternalStation AS ES ON R.RepresentativeExternalStationId = ES.Id

