// Step 1: dotnet add package Google.Cloud.AIPlatform.V1 --version 2.20.0
// Step 2: dotnet add package Google.Apis.Auth --version 1.63.0
// Step 3: Add environment variable in launch.json file as per https://github.com/googleapis/google-cloud-dotnet#authentication
//  - Step 3.1: Ctrl + Shift + P: Select .NET: Generate Assets for Build and Debug
//  - Step 3.2: Add GOOGLE_APPLICATION_CREDENTIALS in launch.json file as env variable for Google Credentials
// Step 4: Add env variables
// Step 5: Change "console": "integratedTerminal" in launch.json 

// See documentation: https://cloud.google.com/vertex-ai/generative-ai/docs/multimodal/overview#c

using System.Text.Json;
using Google.Cloud.AIPlatform.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;
using Google.Protobuf.Collections;
using VertexAISDKGemini;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        string LOCATION = GetEnv("LOCATION");
        string PROJECT_ID = GetEnv("PROJECT_ID");
        string PUBLISHER = GetEnv("PUBLISHER");
        string MODEL_ID = GetEnv("MODEL_ID");

        PredictionServiceClient predictionServiceClient = new PredictionServiceClientBuilder
        {
            Endpoint = $"{LOCATION}-aiplatform.googleapis.com",
            ChannelCredentials = GetCredentials()
        }.Build();

        GenerateContentRequest request = new()
        {
            Model = $"projects/{PROJECT_ID}/locations/{LOCATION}/publishers/{PUBLISHER}/models/{MODEL_ID}",
            Contents = { BuildContents() },
            SystemInstruction = new Content
            {
                Parts =
                {
                    new Part { Text = GetEnv("CONTEXT") }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                CandidateCount = Convert.ToInt32(GetEnv("CANDIDATE_COUNT")),
                Temperature = float.Parse(GetEnv("TEMPERATURE"), CultureInfo.GetCultureInfo("en-US")),
                MaxOutputTokens = Convert.ToInt32(GetEnv("MAX_OUTPUT_TOKENS")),
                TopP = float.Parse(GetEnv("TOP-P"), CultureInfo.GetCultureInfo("en-US")),
                Seed = Convert.ToInt32(GetEnv("SEED"))
            }
        };

        GenerateContentResponse response = predictionServiceClient.GenerateContent(request);
        string modelResponse = response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No Response";

        Console.WriteLine(modelResponse);

        Main(args);
    }

    private static ChannelCredentials GetCredentials()
    {
        string SERVICE_ACCOUNT_FILE_PATH = GetEnv("SERVICE_ACCOUNT_FILE_PATH");
        string SCOPES = GetEnv("SCOPES");

        using Stream stream = new FileStream(SERVICE_ACCOUNT_FILE_PATH, FileMode.Open, FileAccess.Read);

        return GoogleCredential
            .FromStream(stream)
            .CreateScoped(new string[] { SCOPES })
            .UnderlyingCredential
            .ToChannelCredentials();
    }

    private static RepeatedField<Content> BuildContents()
    {
        string jsonContents = File.ReadAllText("contents.json")
            .Replace("{USER_PROMPT}", GetPrompt());

        RepeatedField<Content> contents = new();
        List<MessageContent> messageContentList = JsonSerializer.Deserialize<List<MessageContent>>(jsonContents);

        foreach (MessageContent messageContent in messageContentList)
        {
            contents.Add(new Content
            {
                Role = messageContent.Role,
                Parts =
                {
                    new Part { Text = messageContent.Text }
                }
            });
        }

        return contents;
    }

    private static string GetPrompt()
    {
        Console.WriteLine("Type a prompt:");
        return Console.ReadLine();
    }

    private static string GetEnv(string envName)
    {
        return Environment.GetEnvironmentVariable(envName);
    }
}