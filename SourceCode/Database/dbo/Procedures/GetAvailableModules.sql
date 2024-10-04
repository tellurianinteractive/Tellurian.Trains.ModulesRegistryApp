CREATE PROCEDURE [dbo].[GetAvailableModules]
	@LayoutId INT,
	@PersonId INT NULL
AS
	SET NOCOUNT ON;
	SELECT
		LP.LayoutId,
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
		G.Id AS OwnerGroupId,
		G.FullName AS OwnerGroupName,
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
		ModuleOwnership AS MO ON MO.ModuleId = M.Id LEFT JOIN
		Station AS S ON ISNULL(S.PrimaryModuleId,0) = M.Id INNER JOIN
		Person AS P ON P.Id = ISNULL(MO.PersonId,0) LEFT JOIN
		[Group] AS G ON G.Id = MO.GroupId LEFT JOIN
		LayoutModule AS LM ON LM.ModuleId = M.Id LEFT JOIN
		LayoutParticipant AS LP ON LP.Id = LM.LayoutParticipantId

	WHERE
		LP.LayoutId IS NULL  AND
		ISNULL(M.FunctionalState,0) > 3 AND -- See public enum ModuleFunctionalState
		--LM.Id IS NULL AND -- Module is not registered by any
		(
			ISNULL(MO.PersonId,0) = @PersonId OR 
			ISNULL(MO.GroupId,0) IN (SELECT DISTINCT G.Id FROM [Group] AS G INNER JOIN GroupMember AS GM ON GM.GroupId = G.Id WHERE G.Id = MO.GroupId AND GM.PersonId = @PersonId AND GM.MayBorrowGroupsModules <> 0) OR
			ISNULL(MO.ModuleId,0) IN (SELECT DISTINCT M.Id FROM [Group] AS G INNER JOIN GroupMember AS GM ON GM.GroupId = G.Id INNER JOIN ModuleOwnership AS MOX ON MOX.GroupId = G.Id INNER JOIN Module AS M ON M.Id = MOX.ModuleId WHERE G.Id = MO.GroupId AND GM.MemberMayBorrowMyModules <> 0)
		)
	
	
	RETURN 0
