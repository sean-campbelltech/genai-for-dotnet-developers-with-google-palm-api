using GenAiBot.Models;

namespace GenAiBot.Services
{
    public interface IGooglePaLMService
    {
        PaLMBotViewModel Predict(string prompt);
    }
}