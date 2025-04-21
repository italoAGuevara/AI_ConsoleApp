using Microsoft.AspNetCore.Mvc;
using web_api.Interfaces;

namespace web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IArtificalInteligence _AISerice;

        public AIController(IArtificalInteligence AISerice)
        {
            _AISerice = AISerice;

        }

        [HttpGet("chat", Name = "chat")]
        public async Task<IActionResult> Chat(string inputUser)
        {
            var result = await _AISerice.executeFunctionCaller(inputUser);
            return Ok(result);
        }

        [HttpGet("embeddings", Name = "embeddings")]
        public async Task<IActionResult> Embeddings(string inputUser)
        {
            var result = await _AISerice.executeTextEmbeddings(inputUser);
            return Ok(result);
        }

        [HttpGet("rag", Name = "rag")]
        public async Task<IActionResult> RAG(string inputUser)
        {
            var result = await _AISerice.executeRAG(inputUser);
            return Ok(result);
        }
    }
}
