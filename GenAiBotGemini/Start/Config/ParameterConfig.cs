namespace GenAiBot.Config
{
    public class ParameterConfig
    {
        public float Temperature { get; set; }
        public int MaxOutputTokens { get; set; }
        public float TopP { get; set; }
        public int Seed { get; set; }
    }
}