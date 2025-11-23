using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Data.Api;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[ProducesResponseType(typeof(IEnumerable<CargoType>), 200)]
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
