using Microsoft.Data.SqlClient;
using Rationals;

namespace ModulesRegistry.Services.Implementations;
public class ModuleOwnershipService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public ModuleOwnershipService(IDbContextFactory<ModulesDbContext> factory)
    {
        Factory = factory;
    }

    public async Task<(int Count, string Message, Module? Entity)> TransferOwnershipAsync(ClaimsPrincipal? principal, ModuleOwnership origin, ModuleOwnership newOwnership)
    {
        var share = newOwnership.OwnedShare();
        if (principal.IsAuthenticated() && share > Rational.Zero)
        {
            var transfer = origin.TransferTo(newOwnership);
            if (transfer.Length < 2) return principal.NothingToUpdate<Module>();

            using var dbContext = Factory.CreateDbContext();

            if (transfer[0].OwnedShare < 0.01)
            {
                dbContext.ModuleOwnerships.Remove(origin);
            }
            else
            {
                dbContext.ModuleOwnerships.Entry(origin).CurrentValues.SetValues(transfer[0]);
                dbContext.ModuleOwnerships.Entry(origin).State = EntityState.Modified;
            };
            dbContext.ModuleOwnerships.Add(transfer[1]);
            var result = await dbContext.SaveChangesAsync();
            var module = await GetModule(dbContext, origin.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }
    public async Task<(int Count, string Message, Module? Entity)> AddAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.ModuleOwnerships
                .AsNoTracking()
                .SingleOrDefault(mo => mo.ModuleId == ownership.ModuleId && (ownership.PersonId.HasValue && mo.PersonId == ownership.PersonId || ownership.GroupId.HasValue && mo.GroupId == ownership.GroupId));
            if (existing is not null) return principal.SaveNotAuthorised<Module>();

            var result = await SaveAssistant(dbContext, ownership);
            var module = await GetModule(dbContext, ownership.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }

    private static async Task<Module?> GetModule(ModulesDbContext dbContext, int moduleId) =>
        await dbContext.Modules
           .AsNoTracking()
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
           .SingleOrDefaultAsync(m => m.Id == moduleId)
           .ConfigureAwait(false);


    private static async Task<int> SaveAssistant(ModulesDbContext dbContext, ModuleOwnership ownership)
    {
        var connection = dbContext.Database.GetDbConnection() as SqlConnection;
        if (connection is null) throw new ArgumentException(nameof(connection));

        var sql = "INSERT INTO ModuleOwnership (PersonId, GroupId, ModuleId, OwnedShare) VALUES (@PersonId, @GroupId, @ModuleId, @OwnedShare)";
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = System.Data.CommandType.Text;
        command.Parameters.AddWithValue("@PersonId", ownership.PersonId.AsValueOrDBNull());
        command.Parameters.AddWithValue("@GroupId", ownership.GroupId.AsValueOrDBNull());
        command.Parameters.AddWithValue("@ModuleId", ownership.ModuleId);
        command.Parameters.AddWithValue("@OwnedShare", 0);
        connection.Open();
        var result = await command.ExecuteNonQueryAsync();
        connection.Close();
        return result;
    }

    public async Task<(int Count, string Message)> RemoveAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ModuleOwnerships.SingleOrDefaultAsync(mo => mo.Id == ownership.Id);
            if (existing is null) return principal.NotFound();
            dbContext.ModuleOwnerships.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<ModuleOwnership>();
    }
}
