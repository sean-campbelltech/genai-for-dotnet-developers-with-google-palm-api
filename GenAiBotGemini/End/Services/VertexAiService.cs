// Remember to add GOOGLE_APPLICATION_CREDENTIALS in launchSettings.json / launch.json file as env variable
using System.Text.Json;
using End.Models;
using GenAiBot.Config;
using GenAiBot.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace GenAiBot.Services
{
    public class VertexAiService : IVertexAiService
    {
        private readonly AppConfig _appConfig;
        private readonly List<ChatHistory> _history = new List<ChatHistory>();

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
                GenerationConfig = new GenerationConfig
                {
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
                .Replace("{CONTEXT}", _appConfig.BotConfig.Context)
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

        private GenAiChatViewModel BuildChatMessage(string prompt,
                GenerateContentResponse response)
        {
            string botMessage = response?.Candidates.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "The LLM did not provide a response";

            _history.Add(new ChatHistory("user", prompt));
            _history.Add(new ChatHistory("bot", botMessage));

            return new GenAiChatViewModel(
                _appConfig.BotConfig.BotName,
                _appConfig.BotConfig.Slogan,
                _history);
        }

    }
}