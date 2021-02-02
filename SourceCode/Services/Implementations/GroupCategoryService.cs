using ModulesRegistry.Services.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
#pragma warning disable CA1822 // Mark members as static dont apply because this may calling a database later.
    public class GroupCategoryService : IGroupCategoryService
    {
        public async Task<IEnumerable<(string, string)>> AllAsync() => await Task.FromResult(new []
        {
            (nameof(Strings.Community), Strings.Community),
            (nameof(Strings.NonProfitAssociation), Strings.NonProfitAssociation)
        });
    }
}
