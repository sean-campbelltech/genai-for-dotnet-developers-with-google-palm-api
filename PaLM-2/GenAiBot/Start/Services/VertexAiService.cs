// Remember to add GOOGLE_APPLICATION_CREDENTIALS in launchSettings.json / launch.json file as env variable
using System.Globalization;
using System.Text.Json;
using GenAiBot.Config;
using GenAiBot.Models;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace GenAiBot.Services
{
    public class VertexAiService: IVertexAiService
    {
        private readonly AppConfig _appConfig;

        public VertexAiService(IOptions<AppConfig> appConfigOptions) 
        {
            _appConfig = appConfigOptions.Value;
        }

        public async Task<GenAiChatViewModel> PredictAsync(string prompt)
        {
            PredictionServiceClientBuilder serviceClientBuilder = new PredictionServiceClientBuilder
            {
                Endpoint = _appConfig.PaLMApiConfig.RegionEndpoint
            };
            PredictionServiceClient predictionServiceClient = serviceClientBuilder.Build();
            EndpointName endpoint = EndpointName.FromProjectLocationPublisherModel(
                _appConfig.PaLMApiConfig.Project, 
                _appConfig.PaLMApiConfig.Location, 
                _appConfig.PaLMApiConfig.Publisher, 
                _appConfig.PaLMApiConfig.Model);

            List<Value> instances = GetInstances(prompt);
            Value parameters = GetParameters();
            
            PredictResponse response = await predictionServiceClient.PredictAsync(endpoint, instances, parameters);

            return BuildChatMessage(prompt, response);
        }

        private GenAiChatViewModel BuildChatMessage(string prompt, PredictResponse predictResponse)
        {
            // TODO: Complete implementation of BuildChatMessage()            
            return new GenAiChatViewModel();
        }

        private dynamic[] GetExamples()
        {
            string json = File.ReadAllText("examples.json");
            return JsonSerializer.Deserialize<dynamic[]>(json);
        }

        private List<Value> GetInstances(string prompt)
        {
            return new List<Value>
            {
                new() {
                    StructValue = Struct.Parser.ParseJson(
                        JsonSerializer.Serialize(new 
                        {   
                            context = _appConfig.BotConfig.Context,  
                            examples = GetExamples(),
                            messages = new [] { new 
                                { 
                                    author = "user",
                                    content = prompt
                                }
                            }
                        }))
                }
            };
        }

        private Value GetParameters()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            return new Value()
            {
                StructValue = Struct.Parser.ParseJson(JsonSerializer.Serialize(new 
                {   
                    temperature = _appConfig.ParameterConfig.Temperature,
                    maxOutputTokens = _appConfig.ParameterConfig.MaxOutputTokens,
                    topP = _appConfig.ParameterConfig.TopP,
                    topK = _appConfig.ParameterConfig.TopK
                }))
            };
        }
    }
}