using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

[Route("api/meetings")]
public class MeetingController : ControllerBase
{
    public MeetingController(MeetingService meetingService)
    {
        MeetingService = meetingService;
    }
    private readonly MeetingService MeetingService;


    [Route("{countryId:int?}", Order = 1)]
    public async Task<IActionResult> Index(int? countryId, string? countries)
    {
        var meetings = await MeetingService.GetMeetingsAsync(countryId > 0 ? countryId : null, countries);
        if (meetings.Any()) return Ok(meetings);
        return NotFound();
    }
}
