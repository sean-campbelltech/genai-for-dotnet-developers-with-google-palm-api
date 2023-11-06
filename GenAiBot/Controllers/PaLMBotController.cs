using GenAiBot.Models;
using GenAiBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenAiBot.Controllers
{
    public class PaLMBotController : Controller
    {
        private readonly IGooglePaLMService _palmService;

        public PaLMBotController(IGooglePaLMService paLMService)
        {
            _palmService = paLMService;
        }

        [HttpGet]
        public async Task<IActionResult> Intro()
        {
            PaLMBotViewModel model = await PredictAsync("Who are you and what can you do?");
            return View("Chat", model);
        }

        [HttpPost]
        public async Task<IActionResult> Chat(string prompt)
        {
            PaLMBotViewModel model = await PredictAsync(prompt);
            return View(model);
        }

        public async Task<PaLMBotViewModel> PredictAsync(string prompt)
        {
            return await _palmService.PredictAsync(prompt);
        }
    }
}