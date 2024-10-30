﻿using Microsoft.Data.SqlClient;
using Rationals;

namespace ModulesRegistry.Services.Implementations;
public class ModuleOwnershipService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<(int Count, string Message, Module? Entity)> TransferOwnershipAsync(ClaimsPrincipal? principal, ModuleOwnership origin, ModuleOwnership newOwnership)
    {
        var share = newOwnership.OwnedShare();
        if (principal.IsAuthenticated() && share > Rational.Zero)
        {
            var transfer = new ModuleOwnershipTransfer(origin.AsModuleOwnershipRef(), newOwnership.AsModuleOwnershipRef(), newOwnership.OwnedShare);
            if (transfer.IsZero) return principal.NothingToUpdate<Module>();

            var (From, To) = origin.Transfer(transfer);
            using var dbContext = Factory.CreateDbContext();

            if (From.OwnedShare < 0.01)
            {
                dbContext.ModuleOwnerships.Entry(origin).State = EntityState.Deleted;
            }
            else
            {
                dbContext.ModuleOwnerships.Entry(origin).CurrentValues.SetValues(From);
                dbContext.ModuleOwnerships.Entry(origin).State = EntityState.Modified;
            };
            var existingNewOwnership = await dbContext.ModuleOwnerships
                 .SingleOrDefaultAsync(mo => mo.ModuleId == To.ModuleId && (To.PersonId.HasValue && mo.PersonId == To.PersonId.Value || To.GroupId.HasValue && mo.GroupId == To.GroupId.Value));
            if (existingNewOwnership is null)
            {
                if (To.OwnedShare > 0) dbContext.ModuleOwnerships.Add(To);
            }
            else
            {
                existingNewOwnership.OwnedShare += To.OwnedShare;
                dbContext.ModuleOwnerships.Entry(existingNewOwnership).State = EntityState.Modified;
            }
            var result = await dbContext.SaveChangesAsync();

            var module = await GetModule(origin.ModuleId);
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
            var module = await GetModule(ownership.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }

    public async Task<ModuleOwnership?> GetModuleOwnershipAsync(ClaimsPrincipal? principal, int? moduleId, int personId, int groupId)
    {
        if (moduleId is null) return null;
        if (principal.IsAuthenticated())
        {
            if (personId == 0 && groupId == 0) personId = principal.PersonId();
            using var dbContext = Factory.CreateDbContext();
            var result = await dbContext.ModuleOwnerships
                .Include(mo => mo.Group).ThenInclude(g => g.GroupMembers.Where(gm => gm.IsDataAdministrator))
                .Where(mo => mo.ModuleId == moduleId.Value)
                .ToReadOnlyListAsync();
            if (result.Count == 1) return result[0];
            return result 
                .Where(mo => mo.ModuleId == moduleId.Value && (personId > 0 && mo.PersonId == personId) || (groupId > 0 && mo.GroupId == groupId) || mo.Group.GroupMembers.Any(gm => gm.IsDataAdministrator && gm.PersonId == personId))
                .SingleOrDefault();
        }
        return null;
    }

    private async Task<Module?> GetModule(int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Modules
           .AsNoTracking()
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
           .SingleOrDefaultAsync(m => m.Id == moduleId)
           .ConfigureAwait(false);
    }

    private static async Task<int> SaveAssistant(ModulesDbContext dbContext, ModuleOwnership ownership)
    {
        if (dbContext.Database.GetDbConnection() is not SqlConnection connection) throw new ArgumentException(nameof(connection));

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
