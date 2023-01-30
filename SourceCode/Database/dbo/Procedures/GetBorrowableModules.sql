CREATE PROCEDURE [dbo].[GetBorrowableModules]
	@LayoutId INT,
	@PersonId INT
AS
	SELECT *
	FROM [BorrowableGroupModule] AS BGM 
	WHERE BGM.BorrowerPersonId = @PersonId AND BGM.ModuleId NOT IN (SELECT ModuleId FROM RegisteredModules WHERE LayoutId = @LayoutId) 
RETURN 0
