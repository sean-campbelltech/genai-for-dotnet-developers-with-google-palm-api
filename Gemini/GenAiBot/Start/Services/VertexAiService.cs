// Remember to add GOOGLE_APPLICATION_CREDENTIALS in launchSettings.json / launch.json file as env variable
using System.Text.Json;
using GenAiBot.Config;
using GenAiBot.Models;
using Google.Cloud.AIPlatform.V1;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Grpc.Core;
using Grpc.Auth;
using Google.Protobuf.Collections;
using Start.Models;

namespace GenAiBot.Services
{
    public class VertexAiService : IVertexAiService
    {
        private readonly AppConfig _appConfig;

        public VertexAiService(IOptions<AppConfig> appConfigOptions)
        {
            _appConfig = appConfigOptions.Value;
        }

        public async Task<GenAiChatViewModel> PredictAsync(string prompt)
        {
            PredictionServiceClient predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = _appConfig.ApiConfig.RegionEndpoint,
                ChannelCredentials = GetCredentials()
            }
            .Build();

            ApiConfig apiConfig = _appConfig.ApiConfig;
            GenerateContentRequest generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{_appConfig.ApiConfig.Project}/locations/{apiConfig.Location}/publishers/{apiConfig.Publisher}/models/{apiConfig.Model}",
                Contents = { BuildContents(prompt) },
                SystemInstruction = new Content
                {
                    Parts =
                {
                    new Part { Text =_appConfig.BotConfig.Context }
                }
                },
                GenerationConfig = new GenerationConfig
                {
                    CandidateCount = _appConfig.ParameterConfig.CandidateCount,
                    Temperature = _appConfig.ParameterConfig.Temperature,
                    MaxOutputTokens = _appConfig.ParameterConfig.MaxOutputTokens,
                    TopP = _appConfig.ParameterConfig.TopP,
                    Seed = _appConfig.ParameterConfig.Seed
                }
            };

            GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

            return BuildChatMessage(prompt, response);
        }

        private ChannelCredentials GetCredentials()
        {
            using var stream = new FileStream(_appConfig.ApiConfig.JsonKeyFilePath, FileMode.Open, FileAccess.Read);

            return GoogleCredential
                .FromStream(stream)
                .CreateScoped(PredictionServiceClient.DefaultScopes)
                .UnderlyingCredential
                .ToChannelCredentials();
        }

        private RepeatedField<Content> BuildContents(string prompt)
        {
            string jsonContent = File.ReadAllText("contents.json")
                .Replace("{USER_PROMPT}", prompt);

            List<MessageContent> messageContentList = JsonSerializer.Deserialize<List<MessageContent>>(jsonContent);
            RepeatedField<Content> contents = new RepeatedField<Content>();

            foreach (MessageContent messageContent in messageContentList)
            {
                contents.Add(new Content
                {
                    Role = messageContent.Role,
                    Parts = {
                        new Part { Text = messageContent.Text },
                    }
                });
            }

            return contents;
        }

        private GenAiChatViewModel BuildChatMessage(string prompt, GenerateContentResponse response)
        {
            // TODO: Complete implementation of BuildChatMessage()            
            return new GenAiChatViewModel();
        }
    }
}