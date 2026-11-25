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
		-- Only modules with the layout's theme as their primary or secondary theme.
		AND EXISTS (SELECT 1 FROM Module AS M INNER JOIN Layout AS TL ON TL.Id = @LayoutId WHERE M.Id = BGM.ModuleId AND (M.PrimaryThemeId = TL.ThemeId OR M.SecondaryThemeId = TL.ThemeId))
RETURN 0
