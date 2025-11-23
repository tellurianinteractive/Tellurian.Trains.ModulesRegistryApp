using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services;

namespace ModulesRegistry.Api;

[ProducesResponseType(typeof(IEnumerable<string>), 200)]
[Route("api/languages")]
[ApiController]
public class LanguageController(ILanguageService service) : ControllerBase
{
    readonly ILanguageService Service = service;

    [HttpGet("all/supported")]
    public IActionResult GetSupportedLanguages() => Ok(Service.GetSupportedLanguages());

}
