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

internal class Program
{
    private static string LOCATION = GetEnv("LOCATION");
    private static string PROJECT_ID = GetEnv("PROJECT_ID");
    private static string MODEL_ID = GetEnv("MODEL_ID");
    private static string PUBLISHER = GetEnv("PUBLISHER");
    private static string CONTEXT = GetEnv("CONTEXT");
    private static string SERVICE_ACCOUNT_FILE_PATH = GetEnv("SERVICE_ACCOUNT_FILE_PATH");

    private static void Main(string[] args)
    {
        PredictionServiceClient predictionServiceClient = new PredictionServiceClientBuilder
        {
            Endpoint = $"{LOCATION}-aiplatform.googleapis.com",
            ChannelCredentials = GetCredentials(SERVICE_ACCOUNT_FILE_PATH)
        }.Build();

        GenerateContentRequest generateContentRequest = new GenerateContentRequest
        {
            Model = $"projects/{PROJECT_ID}/locations/{LOCATION}/publishers/{PUBLISHER}/models/{MODEL_ID}",
            Contents = { BuildContents() }
        };

        GenerateContentResponse response = predictionServiceClient.GenerateContent(generateContentRequest);
        string modelResponse = response?.Candidates.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No Response";

        Console.WriteLine(modelResponse);

        Main(args);
    }

    private static ChannelCredentials GetCredentials(string jsonKeyFilePath)
    {
        string SCOPES = GetEnv("SCOPES");
        using var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read);

        return GoogleCredential
            .FromStream(stream)
            .CreateScoped(new string[] { SCOPES })
            .UnderlyingCredential
            .ToChannelCredentials();
    }

    private static RepeatedField<Content> BuildContents()
    {
        string jsonContent = File.ReadAllText("contents.json")
            .Replace("{CONTEXT}", CONTEXT)
            .Replace("{USER_PROMPT}", GetPrompt());

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

    private static string GetPrompt()
    {
        Console.WriteLine("Type a prompt:");
        return Console.ReadLine();
    }

    private static string GetEnv(string env)
    {
        return Environment.GetEnvironmentVariable(env);
    }
}