using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[Route("api/cargoes")]
[ApiController]
public class CargoController : ControllerBase
{
    public CargoController(CargoService cargoService)
    {
        CargoService = cargoService;
    }
    private readonly CargoService CargoService;

    [Route("all")]
    public async Task<IActionResult> Index()
    {
        var cargoTypes = await CargoService.CargoTypesAsync();
        if (cargoTypes.Any()) return Ok(cargoTypes);
        return NotFound();
    }
}
