using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[ProducesResponseType(typeof(IEnumerable<LayoutVehicle>), 200)]
[Route("api/layouts")]
[ApiController]
public class LayoutController(LayoutService layoutService) : Controller
{
    private LayoutService LayoutService { get; } = layoutService;
    public Task<AuthenticationState>? AuthenticationStateTask { get; }

    [Route("{layoutId:int}/vehicles", Order = 1)]
    public async Task<IActionResult> Index(int layoutId, [FromQuery] bool tractionOnly)
    {
        var layoutVehicles = await LayoutService.GetLayoutVehicles(layoutId, tractionOnly);
        if (layoutVehicles.Any()) return Ok(layoutVehicles);
        return NotFound();
    }
}
