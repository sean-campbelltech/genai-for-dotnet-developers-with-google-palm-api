using GenAiBot.Models;

namespace GenAiBot.Services
{
    public interface IVertexAiService
    {
        Task<GenAiChatViewModel> PredictAsync(string prompt);
    }
}