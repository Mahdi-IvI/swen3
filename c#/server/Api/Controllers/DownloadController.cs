using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DownloadController : ControllerBase
{
    [HttpGet("{documentId}", Name = "DownloadDocument")]
    public IActionResult Download(int documentId)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
