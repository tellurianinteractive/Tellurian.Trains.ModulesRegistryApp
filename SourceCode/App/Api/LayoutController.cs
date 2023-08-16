using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services.Implementations;
namespace ModulesRegistry.Api;

[Route("api/layouts")]
public class LayoutController : Controller
{
    public LayoutController(LayoutService layoutService)
    {
        LayoutService = layoutService;
    }
    private LayoutService LayoutService { get; }
    public Task<AuthenticationState>? AuthenticationStateTask { get; }

    [Route("{layoutId:int}/vehicles", Order = 1)]
    public async Task<IActionResult> Index(int layoutId, [FromQuery] bool tractionOnly)
    {
        var layoutVehicles = await LayoutService.GetLayoutVehicles(layoutId, tractionOnly);
        if (layoutVehicles.Any()) return Ok(layoutVehicles);
        return NotFound();
    }
}
