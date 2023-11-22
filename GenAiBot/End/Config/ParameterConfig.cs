namespace GenAiBot.Config
{
    public class ParameterConfig
    {
        public double Temperature { get; set; }
        public int MaxOutputTokens { get; set; }
        public double TopP { get; set; }
        public int TopK { get; set; }
    }
}