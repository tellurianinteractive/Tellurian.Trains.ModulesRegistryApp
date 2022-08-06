namespace ModulesRegistry.Services.Implementations;

public class PropertyService
{
    private const string EndProfile = "EndProfile";
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public PropertyService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public Task<IEnumerable<ListboxItem>> GetEndProfileListboxItemsAsync() => GetListboxItemsAsync(EndProfile);
    public Task<int> RemoveEndProfile(string value) => RemoveProperty(EndProfile, value);
    public Task<IEnumerable<ListboxItem>> AddEndProfile(string value) => AddProperty(EndProfile, value);


    public async Task<IEnumerable<ListboxItem>> GetListboxItemsAsync(string name)
    {
        using var dbContext = Factory.CreateDbContext();
        var items = await dbContext.Properties.AsNoTracking().Where(p => p.Name.Equals(name)).Select(p => new ListboxItem(p.Id, p.Value)).ToListAsync();
        return items.OrderBy(i => i.Description);
    }

    public async Task<IEnumerable<ListboxItem>> AddProperty(string name, string value)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Properties.SingleOrDefaultAsync(p => p.Name == name && p.Value == value);
        if (existing is null)
        {
            dbContext.Add(new Property { Name = name, Value = value });
            await dbContext.SaveChangesAsync();
        }
        return await GetListboxItemsAsync(name);
    }

    private async Task<int> RemoveProperty(string name, string value)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Properties.SingleOrDefaultAsync(p => p.Name == name && p.Value == value);
        if (existing is null) return 0;
        dbContext.Remove(existing);
        return await dbContext.SaveChangesAsync();
    }
}
