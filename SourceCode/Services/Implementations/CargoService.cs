using Microsoft.AspNetCore.Components;

namespace ModulesRegistry.Services.Implementations;

public sealed class CargoService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public CargoService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<Data.Api.CargoType>> CargoTypesAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        var result = await dbContext.Cargos.ToReadOnlyListAsync();
        if (result is null) return Array.Empty<Data.Api.CargoType>();
        return result
            .Select(c => new Data.Api.CargoType(c.Id, c.NhmCode, c.DefaultClasses) { Translations = c.LocalizedNames().Select(ln => new Data.Api.Translation(ln.Language, ln.Value)) }).ToList();
    }

    public async Task<IEnumerable<ListboxItem>> CargoListboxItemsAsync(ClaimsPrincipal? principal, bool includeDefaultClasses = true)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Cargos
                .ToReadOnlyListAsync();
            if (includeDefaultClasses)
            {
                return items.Select(c => new ListboxItem(c.Id, $"{c.MajorNhmCode()} {c.LocalizedName().Value} ({c.DefaultClasses})"))
                    .OrderBy(l => l.Description).ToList();
            }
            else
            {
                return items.Select(c => new ListboxItem(c.Id, $"{c.MajorNhmCode()} {c.LocalizedName().Value}"))
                    .OrderBy(l => l.Description).ToList();

            }
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> CargoDirectionsListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.CargoDirections
                .Select(cd => new ListboxItem(cd.Id, cd.FullName.AsLocalized())).ToReadOnlyListAsync();
            return items
                .OrderBy(l => l.Description).ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> CargoQuantityListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.CargoUnits
                .Select(cu => new ListboxItem(cu.Id, cu.FullName.AsLocalized())).ToReadOnlyListAsync();
            return items.OrderBy(l => l.Description).ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> ReadyTimeListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.CargoReadyTimes
                .Select(crt => new ListboxItem(crt.Id, crt.FullName.AsLocalized())).ToReadOnlyListAsync();
            return items.OrderBy(l => l.Id).ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> CargoPackagingUnitListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.CargoPackagingUnits
                .Select(cpu => new ListboxItem(cpu.Id, cpu.PluralResourceCode.AsLocalized()) { DisplayOrder = cpu.DisplayOrder }).ToReadOnlyListAsync();
            return items.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Description);
        }
        return Array.Empty<ListboxItem>();

    }

    public async Task<IEnumerable<Cargo>> GetAll()
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Cargos.ToReadOnlyListAsync();
    }

    public async Task<Cargo?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal is null) return null;
        using var dbContext = Factory.CreateDbContext();
        if (principal.IsAnyAdministrator()) return await dbContext.Cargos.FindAsync(id).ConfigureAwait(false);
        return null;
    }

    public async Task<(int Count, string Message, Cargo? Entity)> SaveAsync(ClaimsPrincipal? principal, Cargo entity)
    {
        if (principal is null) return principal.SaveNotAuthorised<Cargo>();
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Cargos.FindAsync(entity.Id).ConfigureAwait(false);
        if (existing is null)
        {
            dbContext.Cargos.Add(entity);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(entity);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);
        }
    }
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal is null) return principal.DeleteNotAuthorized<Cargo>();
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Cargos.FindAsync(id).ConfigureAwait(false);
        if (existing is null) return principal.NotFound();
        dbContext.Remove(existing);
        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
        return result.DeleteResult();
    }

    #region NHM

    public async Task<IEnumerable<ListboxItem>> GetNhmItems(ClaimsPrincipal? principal, int? subItemsToId = null)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            if (subItemsToId.HasValue)
            {
                if (subItemsToId.Value == 0) return await dbContext.NhmCodes
                        .Where(c => c.LevelDigits <= 4)
                        .OrderBy(c => c.Id)
                        .Select(c => ListboxItem(c))
                        .ToReadOnlyListAsync();
                var min = subItemsToId + 1;
                var max = min + 999999;
                return await dbContext.NhmCodes
                    .Where(c => c.Id >= min && c.Id <= max)
                    .OrderBy(c => c.Id).Select(c => ListboxItem(c))
                    .ToReadOnlyListAsync();
            }
            else
            {
                return await dbContext.NhmCodes
                    .Where(c => c.LevelDigits == 2)
                    .OrderBy(c => c.Id)
                    .Select(c => ListboxItem(c))
                    .ToReadOnlyListAsync();
            }
        }
        return Array.Empty<ListboxItem>();

    }

    private static ListboxItem ListboxItem(NHM nhm) => new(nhm.Id, Description(nhm));

    private static string Description(NHM nhm) => $"{nhm.Code![..nhm.LevelDigits]} {nhm.LocalizedName().Value.LineWrapped(200)}";

    #endregion
}
