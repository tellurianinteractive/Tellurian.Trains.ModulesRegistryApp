namespace ModulesRegistry.Services.Implementations;

public class ModuleEndProfileService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(int? scaleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.ModuleEndProfiles.AsNoTracking()
            .Where(mgt => !scaleId.HasValue || mgt.ScaleId == scaleId)
            .Select(mgt => new ListboxItem(mgt.Id, mgt.Designation))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ModuleEndProfile>> GetAllAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.ModuleEndProfiles.AsNoTracking()
            .Include(mgt => mgt.Scale)
            .Include(mgt => mgt.ModuleStandard)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>An end profile to edit; only global administrators edit end profiles.</summary>
    public async Task<ModuleEndProfile?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsGlobalAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleEndProfiles.FindAsync(id).ConfigureAwait(false);
        }
        return null;
    }

    /// <summary>
    /// The drawing of an end profile (<see cref="EndProfileDrawings"/>), for every signed-in user, with the Registry's id written into
    /// it (<see cref="EndProfileDrawings.WithRegistryData"/>); null when the end profile has none.
    /// </summary>
    public async Task<(ModuleEndProfile EndProfile, byte[] Content)?> FindDrawingAsync(ClaimsPrincipal? principal, int endProfileId)
    {
        if (!principal.IsAuthenticated()) return null;
        using var dbContext = Factory.CreateDbContext();
        var profile = await dbContext.ModuleEndProfiles.AsNoTracking()
            .Include(p => p.Scale)
            .Include(p => p.ModuleStandard)
            .SingleOrDefaultAsync(p => p.Id == endProfileId)
            .ConfigureAwait(false);
        if (profile?.DrawingId is not int documentId) return null;
        var content = await dbContext.Documents.Where(d => d.Id == documentId).Select(d => d.Content).SingleOrDefaultAsync().ConfigureAwait(false);
        if (content is null) return null;
        return (profile, EndProfileDrawings.WithRegistryData(content, profile.Id));
    }

    /// <summary>
    /// For downloading a document: whether it is an end profile's drawing, and if so its content as <see cref="FindDrawingAsync"/> gives it.
    /// </summary>
    public async Task<(bool IsEndProfileDrawing, byte[]? Content)> FindDrawingByDocumentIdAsync(ClaimsPrincipal? principal, int documentId)
    {
        using var dbContext = Factory.CreateDbContext();
        var endProfileId = await dbContext.ModuleEndProfiles.Where(p => p.DrawingId == documentId).Select(p => (int?)p.Id).SingleOrDefaultAsync().ConfigureAwait(false);
        if (endProfileId is null) return (false, null);
        return (true, (await FindDrawingAsync(principal, endProfileId.Value).ConfigureAwait(false))?.Content);
    }

    /// <summary>Adds or updates an end profile; only global administrators (as before: <c>IsGlobalOrCountryAdministrator()</c> without a country). The documents are kept as they are. With a module standard, the end profile gets the standard's scale.</summary>
    public async Task<(int Count, string Message, ModuleEndProfile? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleEndProfile entity)
    {
        if (principal.IsGlobalAdministrator() && !principal.IsReadOnly())
        {
            entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
            using var dbContext = Factory.CreateDbContext();
            if (entity.ModuleStandardId is int standardId)
            {
                var standardScaleId = await dbContext.ModuleStandards.Where(ms => ms.Id == standardId).Select(ms => (int?)ms.ScaleId).SingleOrDefaultAsync().ConfigureAwait(false);
                // The scale follows the standard (corrected, not validated); a standard deleted meanwhile is dropped.
                if (standardScaleId is int scaleId) entity.ScaleId = scaleId;
                else entity.ModuleStandardId = null;
            }
            var existing = await dbContext.ModuleEndProfiles.FindAsync(entity.Id).ConfigureAwait(false);
            if (existing is null)
            {
                entity.PdfDocumentId = null;
                entity.DrawingId = null;
                dbContext.ModuleEndProfiles.Add(entity);
            }
            else
            {
                // Documents are uploaded and deleted on their own (DocumentService), never changed here.
                entity.PdfDocumentId = existing.PdfDocumentId;
                entity.DrawingId = existing.DrawingId;
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            }
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing ?? entity);
        }
        return principal.SaveNotAuthorised<ModuleEndProfile>();
    }
}