CREATE PROCEDURE [dbo].[GetRegisteredModules]
	@LayoutId INT,
	@PersonId INT = NULL
AS
	SET NOCOUNT ON;
	SELECT * 
	FROM [RegisteredModules] AS RM 
	WHERE RM.LayoutId = @LayoutId AND (@PersonId IS NULL OR RM.PersonId = @PersonId)
	ORDER BY ModuleName
RETURN 0
