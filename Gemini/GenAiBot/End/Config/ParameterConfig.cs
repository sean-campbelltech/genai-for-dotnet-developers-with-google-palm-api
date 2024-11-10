namespace GenAiBot.Config
{
    public class ParameterConfig
    {
        public int CandidateCount { get; set; }
        public float Temperature { get; set; }
        public int MaxOutputTokens { get; set; }
        public float TopP { get; set; }
        public int Seed { get; set; }
    }
}