CREATE PROCEDURE [dbo].[GetAvailableModules]
	@LayoutId INT,
	@PersonId INT
AS
	SET NOCOUNT ON;
	SELECT
		M.Id AS ModuleId,
		M.FullName AS ModuleName,
		M.ScaleId,
		M.StandardId,
		M.FunctionalState,
		M.Is2R,
		M.Is3R,
		M.LandscapeSeason,
		M.PackageLabel,
		M.ConfigurationLabel,
		M.FremoNumber,
		S.Id AS StationId,
		MO.OwnedShare,
		P.Id AS OwnerPersonId,
		P.FirstName AS OwnerFirstName,
		P.LastName AS OwnerLastName,
		NULL AS OwnerGroupId,
		NULL AS OwnerGroupName,
		P.Id AS PersonId,
		P.FirstName,
		P.MiddleName,
		P.LastName,
		P.FremoOwnerSignature,
		P.CityName,
		P.CountryId,
		P.UserId
	FROM 
		Module AS M INNER JOIN
		ModuleOwnership AS MO ON MO.ModuleId = M.Id INNER JOIN
		Person AS P ON P.Id = MO.PersonId LEFT JOIN
		Station AS S ON S.PrimaryModuleId = M.Id
	WHERE
		MO.OwnedShare > 0 AND
		M.FunctionalState > 3 AND -- See public enum ModuleFunctionalState
		MO.PersonId = @PersonId AND
		@LayoutId NOT IN (SELECT LP.LayoutId FROM LayoutParticipant AS LP INNER JOIN LayoutModule LM ON LM.LayoutParticipantId = LP.Id AND LM.ModuleId = M.Id AND LP.PersonId = @PersonId)
	
	RETURN 0
