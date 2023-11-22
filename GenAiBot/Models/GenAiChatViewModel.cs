namespace GenAiBot.Models
{
    public class GenAiChatViewModel
    {
        public GenAiChatViewModel(string botName, string slogan, List<ChatHistory> chatHistory)
        {
            BotName = botName;
            Slogan = slogan;
            ChatHistory = chatHistory;
        }

        public string BotName { get; set; }
        public string Slogan { get; set; }
        public List<ChatHistory> ChatHistory { get; set; }
    }
}