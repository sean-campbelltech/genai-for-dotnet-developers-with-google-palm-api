using GenAiBot.Models;

namespace GenAiBot.Services
{
    public interface IGooglePaLMService
    {
        Task<PaLMBotViewModel> PredictAsync(string prompt);
    }
}