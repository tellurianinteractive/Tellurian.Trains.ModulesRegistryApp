using ModulesRegistry.Services.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
#pragma warning disable CA1822 // Mark members as static dont apply because this may calling a database later.
    public sealed class GroupCategoryService
    {
        public async Task<IEnumerable<(string, string)>> AllAsync() => await Task.FromResult(new[]
        {
            (nameof(Strings.Community), Strings.Community),
            (nameof(Strings.NonProfitAssociation), Strings.NonProfitAssociation)
        });
    }
}
