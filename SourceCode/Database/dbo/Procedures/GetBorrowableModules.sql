CREATE PROCEDURE [dbo].[GetBorrowableModules]
	@LayoutId INT,
	@PersonId INT
AS
	SET NOCOUNT ON;
	SELECT *
	FROM [BorrowableGroupModule] AS BGM 
	WHERE BGM.PersonId = @PersonId 
		AND (BGM.OwnerPersonId IS NULL OR BGM.OwnerPersonId <> @PersonId )
		AND BGM.ModuleId NOT IN (SELECT ModuleId FROM RegisteredModules WHERE LayoutId = @LayoutId) 
RETURN 0
