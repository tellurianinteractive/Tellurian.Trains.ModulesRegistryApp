using ModulesRegistry.Services.Localization;

namespace ModulesRegistry.Services.Extensions;

/// <summary>The casing rules of the country a station belongs to (<see cref="AppLanguages.CasingsOf"/>), for correcting its texts.</summary>
public static class CountryCasingExtensions
{
    extension(ModulesDbContext dbContext)
    {
        /// <summary>
        /// The rules of the region's country, or of the user's country when no region is chosen (a station's region is optional);
        /// none when neither is known.
        /// </summary>
        public async Task<TextCasings> CountryCasingsAsync(int? regionId, ClaimsPrincipal? principal)
        {
            var languages = regionId > 0 ?
                await dbContext.Regions.AsNoTracking().Where(r => r.Id == regionId).Select(r => r.Country.Languages).SingleOrDefaultAsync() :
                null;
            var countryId = principal.CountryId();
            if (languages is null && countryId > 0)
                languages = await dbContext.Countries.AsNoTracking().Where(c => c.Id == countryId).Select(c => c.Languages).SingleOrDefaultAsync();
            return AppLanguages.CasingsOf(languages);
        }
    }
}
