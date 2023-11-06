using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenAiBot.Config;
using GenAiBot.Models;
using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;

namespace GenAiBot.Services
{
    public class GooglePaLMService: IGooglePaLMService
    {
        private readonly PaLMApiConfig _apiConfig;

        public GooglePaLMService(IOptions<PaLMApiConfig> apiConfig) 
        {
            _apiConfig = apiConfig.Value;
        }

        public PaLMBotViewModel Predict(string prompt)
        {
            var serviceClientBuilder = new PredictionServiceClientBuilder
            {
                Endpoint = _apiConfig.RegionEndpoint
            };

            PredictionServiceClient predictionServiceClient = serviceClientBuilder.Build();
            EndpointName endpoint = EndpointName.FromProjectLocationPublisherModel(
                _apiConfig.Project, _apiConfig.Location, _apiConfig.Publisher, _apiConfig.Model);
            List<Google.Protobuf.WellKnownTypes.Value> instances = GetInstances(prompt);
            Google.Protobuf.WellKnownTypes.Value parameters = GetParameters();
            PredictResponse response = predictionServiceClient.Predict(endpoint, instances, parameters);

            string botMessage = response.Predictions[0].StructValue.Fields["content"].StringValue;

            return new PaLMBotViewModel
            {
                BotName = "TestBot",
                Slogan = "Test Slogan",
                Message = botMessage,
                Prompt = prompt
            };
        }

        private List<Google.Protobuf.WellKnownTypes.Value> GetInstances(string prompt)
        {
            throw new NotImplementedException();
        }

        private Google.Protobuf.WellKnownTypes.Value GetParameters()
        {
            throw new NotImplementedException();
        }
    }
}