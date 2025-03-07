CREATE PROCEDURE [dbo].[ChangeAdministratorPrivilege]
	@global bit = 0,
	@Country bit = 1,
	@userId int = 1
AS
	UPDATE [User] SET IsGlobalAdministrator = @global, IsCountryAdministrator = @country WHERE Id = @userId
RETURN 0
