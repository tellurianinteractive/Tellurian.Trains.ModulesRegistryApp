using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[Route("api/cargoes")]
[ApiController]
public class CargoController(CargoService cargoService) : ControllerBase
{
    private readonly CargoService CargoService = cargoService;

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var cargoTypes = await CargoService.CargoTypesAsync();
        if (cargoTypes.Any()) return Ok(cargoTypes);
        return NotFound();
    }
}
