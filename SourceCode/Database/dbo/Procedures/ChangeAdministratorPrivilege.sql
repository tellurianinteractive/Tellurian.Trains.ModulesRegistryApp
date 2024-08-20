CREATE PROCEDURE [dbo].[ChangeAdministratorPrivilege]
	@value bit = 0,
	@userId int = 1
AS
	UPDATE [User] SET IsGlobalAdministrator = @value, IsCountryAdministrator = @value WHERE Id = @userId
RETURN 0
