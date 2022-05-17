CREATE VIEW dbo.ListExternalStation
AS
SELECT TOP (100) PERCENT 
    ES.Id,
    ES.FullName + ', ' + R.LocalName + ' (' + C.DomainSuffix + ')' AS [Description], 
    R.Id AS RegionId, 
    R.CountryId,
    R.BackColor,
    R.ForeColor
FROM 
    ExternalStation ES INNER JOIN
    Region R ON ES.RegionId = R.Id INNER JOIN
    Country C ON C.Id = R.CountryId
ORDER BY 
    ES.FullName
