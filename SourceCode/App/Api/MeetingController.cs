using Microsoft.AspNetCore.Mvc;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Api
{
    [Route("api/meetings")]
    public class MeetingController : ControllerBase
    {
        public MeetingController(MeetingService meetingService)
        {
            MeetingService = meetingService;
        }
        private readonly MeetingService MeetingService;

        [Route("{countryid:int}")]
        public async Task<IActionResult> Index(int countryId)
        {
            var meetings = await MeetingService.Meetings(countryId > 0 ? countryId : null);
            if (meetings.Any()) return Ok(meetings);
            return NotFound();
        }
    }
}
