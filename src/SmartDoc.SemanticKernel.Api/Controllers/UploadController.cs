#region using

using System.Threading.Tasks;
using SmartDoc.SemanticKernel.Ingestion.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

#endregion

namespace SmartDoc.SemanticKernel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(PdfChunkService chunkService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file");
        }

        await using var stream = file.OpenReadStream();

        await chunkService.ProcessPdfAndSaveAsync(stream, file.FileName, cancellationToken);

        return Ok("Uploaded and processed");
    }
}