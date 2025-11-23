using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[ProducesResponseType(typeof(IEnumerable<Meeting>), 200)]
[Route("api/meetings")]
[ApiController]
public class MeetingController(MeetingService meetingService) : ControllerBase
{
    private readonly MeetingService MeetingService = meetingService;


    [Route("{countryId:int?}", Order = 1)]
    public async Task<IActionResult> Index(int? countryId, string? countries)
    {
        var meetings = await MeetingService.GetMeetingsAsync(countryId > 0 ? countryId : null, countries);
        if (meetings.Any()) return Ok(meetings);
        return NotFound();
    }
}
