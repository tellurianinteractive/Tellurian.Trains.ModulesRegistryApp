using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class LayoutService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public LayoutService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        /// <summary>
        /// Adds <see cref="LayoutModule">modules</see> and <see cref="LayoutService">stations</see> in a <see cref="ModulePackage"/> to a <see cref="Layout"/>
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="participantId"></param>
        /// <param name="layoutId"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public async Task<(int Count, string Message)> AddPackageModulesAsync(ClaimsPrincipal? principal, int participantId, int layoutId,  ModulePackage package)
        {
            if (principal.IsAuthenticated())
            {
                var result = package.Modules.Count();
                using var dbContext = Factory.CreateDbContext();
                var participant = await dbContext.MeetingParticipants.Include(mp => mp.Meeting).SingleOrDefaultAsync(mp => mp.Id == participantId);
                if (participant is null) return (-1, Resources.Strings.NotFound);
                foreach (var module in package.Modules)
                {
                    var existing = await dbContext.LayoutModules.SingleOrDefaultAsync(lm => lm.ModuleId == module.Id);
                    if (existing is null)
                    {
                        var addedModule = new LayoutModule { LayoutId = layoutId, ModuleId = module.Id, ParticipantId = participant.Id };
                        dbContext.LayoutModules.Add(addedModule);
                        await AddLayoutStationAsync(dbContext, participantId, layoutId, module);
                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        result--;
                    }
                }
                return result == 0 ? (-1, Resources.Strings.NoModification) : (1, Resources.Strings.Saved);

                static async Task AddLayoutStationAsync(ModulesDbContext dbContext, int participantId, int layoutId, Module module)
                {
                    if (module.StationId.HasValue)
                    {
                        var existing = await dbContext.LayoutStations.SingleOrDefaultAsync(ls => ls.LayoutId == layoutId && ls.StationId == module.StationId);
                        if (existing is null)
                        {
                            var addedStation = new LayoutStation { LayoutId = layoutId, StationId = module.StationId.Value };
                            dbContext.LayoutStations.Add(addedStation);
                        }
                    }
                }

            }
            return (0, Resources.Strings.NotAuthorized);
        }

        public async Task<IEnumerable<LayoutModule>> GetRegisteredModulesAsync(ClaimsPrincipal? principal, int participantId, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.LayoutModules.Where(lm => lm.ParticipantId == participantId && lm.LayoutId == layoutId).ToListAsync();
            }
            return Array.Empty<LayoutModule>();
        }

    }
}
