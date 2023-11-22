using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenAiBot.Config
{
    public class AppConfig
    {
        public PaLMApiConfig PaLMApiConfig { get; set; }
        public ParameterConfig ParameterConfig { get; set; }
        public BotConfig BotConfig { get; set; }
    }
}