using GenAiBot.Models;
using GenAiBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenAiBot.Controllers
{
    public class GenAiChatController : Controller
    {
        private readonly IVertexAiService _palmService;

        public GenAiChatController(IVertexAiService paLMService)
        {
            _palmService = paLMService;
        }

        [HttpGet]
        public async Task<IActionResult> Intro()
        {
            return await Chat("Who are you and what can you do?");
        }

        [HttpPost]
        public async Task<IActionResult> Chat(string prompt)
        {
            GenAiChatViewModel model = await _palmService.PredictAsync(prompt);
            return View("GenAiChat", model);
        }
    }
}