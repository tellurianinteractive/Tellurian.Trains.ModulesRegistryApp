CREATE PROCEDURE [dbo].[GetBorrowableModules]
	@LayoutId INT,
	@PersonId INT
AS
	SELECT *
	FROM [BorrowableGroupModule] AS BM
	WHERE BM.PersonId = @PersonId AND BM.LayoutId <> @LayoutId
RETURN 0
