using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services;

namespace ModulesRegistry.Api
{
    [Route("api/languages")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        public LanguageController(ILanguageService service)
        {
            Service = service;
        }
        readonly ILanguageService Service;

        [HttpGet("all/supported")]
        public IActionResult GetSupportedLanguages() => Ok(Service.GetSupportedLanguages());

        [HttpGet("all/labels/waybills")]
        public IActionResult GetWaybillLabels() => Ok(Service.GetWaybillLabes());
    }
}
