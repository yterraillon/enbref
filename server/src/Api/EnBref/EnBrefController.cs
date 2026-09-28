using EnBref.Application.Features;

namespace Api.EnBref;

[Route("api/[controller]")]
[ApiController]
public class EnBrefController(ISender mediator) : ControllerBase
{
    [HttpGet("en-bref")]
    public async Task<IActionResult> GetRecap()
    {
        var request = new GenerateDailyRecap.Request();

        var response = await mediator.Send(request);
        return Ok(response);
    }
}
