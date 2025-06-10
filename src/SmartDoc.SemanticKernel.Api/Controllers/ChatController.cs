using SmartDoc.SemanticKernel.Ingestion.OpenAi;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmartDoc.SemanticKernel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(OpenAiChatService chatService) : ControllerBase
{
    [HttpPost(nameof(Ask))]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        var answer = await chatService.AskAsync(request.Question);
        return Ok(new ChatResponse { Answer = answer });
    }
}

public class ChatRequest
{
    public string Question { get; set; }
}

public class ChatResponse
{
    public string Answer { get; set; }
}