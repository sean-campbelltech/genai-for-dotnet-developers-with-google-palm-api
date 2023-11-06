// Remember to add GOOGLE_APPLICATION_CREDENTIALS in launchSettings.json / launch.json file as env variable
using System.Globalization;
using GenAiBot.Config;
using GenAiBot.Models;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;

namespace GenAiBot.Services
{
    public class GooglePaLMService: IGooglePaLMService
    {
        private readonly AppConfig _appConfig;

        public GooglePaLMService(IOptions<AppConfig> appConfigOptions) 
        {
            _appConfig = appConfigOptions.Value;
        }

        public async Task<PaLMBotViewModel> PredictAsync(string prompt)
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

            List<Google.Protobuf.WellKnownTypes.Value> instances = GetInstances(prompt);
            Google.Protobuf.WellKnownTypes.Value parameters = GetParameters();
            
            PredictResponse response = await predictionServiceClient.PredictAsync(endpoint, instances, parameters);
            string botMessage = response.Predictions[0].StructValue.Fields["content"].StringValue;

            return new PaLMBotViewModel
            {
                BotName = _appConfig.BotConfig.BotName,
                Slogan = _appConfig.BotConfig.Slogan,
                Message = botMessage,
                Prompt = prompt
            };
        }

        private List<Google.Protobuf.WellKnownTypes.Value> GetInstances(string prompt)
        {
            return new List<Google.Protobuf.WellKnownTypes.Value>
            {
                new Google.Protobuf.WellKnownTypes.Value
                {
                    StructValue = Struct.Parser.ParseJson($@"
                            {{
                                ""content"": ""{_appConfig.BotConfig.Context}\n\n input: {prompt}\n output:""
                            }}")
                }
            };
        }

        private Google.Protobuf.WellKnownTypes.Value GetParameters()
        {
            CultureInfo us = new CultureInfo("en-US", false);
            return new Google.Protobuf.WellKnownTypes.Value()
            {
                StructValue = Struct.Parser.ParseJson($@"
                {{
                    ""temperature"": {_appConfig.ParameterConfig.Temperature.ToString(us)},
                    ""maxOutputTokens"": {_appConfig.ParameterConfig.MaxOutputTokens},
                    ""topP"": {_appConfig.ParameterConfig.TopP.ToString(us)},
                    ""topK"": {_appConfig.ParameterConfig.TopK}
                }}")
            };
        }
    }
}