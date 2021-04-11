using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class OperatingDayService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private static readonly ResourceManager ResourceManager = Resources.Strings.ResourceManager;

        public OperatingDayService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<ListboxItem>> BasicDaysItemsAsync()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.OperatingDays.AsNoTracking()
                .Where(od => od.IsBasicDay)
                .OrderBy(od => od.Flag)
                .Select(od => new ListboxItem(od.Id, od.Flag.OperationDays().ShortName))
                .ToListAsync();
        }

        public async Task<IEnumerable<ListboxItem>> AllDaysItemsAsync()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.OperatingDays.AsNoTracking()
                .OrderBy(od => od.Flag)
                .Select(od => new ListboxItem(od.Id, od.Flag.OperationDays().ShortName))
                .ToListAsync();
        }


        private static string Description(OperatingDay day)
        {
            var localized = ResourceManager.GetString(day.FullName);
            return string.IsNullOrWhiteSpace(localized) ? day.FullName : localized;
        }
    }
}
