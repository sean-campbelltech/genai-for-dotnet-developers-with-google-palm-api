// Step 1: dotnet add package Google.Apis.Auth --version 1.68.0
// Step 2: Ctrl + Shift + P -> Select .NET: Generate Assets for Build & Debug
// Step 3: Add env variables
// Step 4: Set "console": "integratedTerminal" in launch.json
// Step 5: Convert to classic Main method to call recursively

using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        string API_ENDPOINT = GetEnv("API_ENDPOINT");
        string PROJECT_ID = GetEnv("PROJECT_ID");
        string MODEL_ID = GetEnv("MODEL_ID");
        string LOCATION_ID = GetEnv("LOCATION_ID");
        string PUBLISHER = GetEnv("PUBLISHER");
        string SERVICE_ACCOUNT_FILE_PATH = GetEnv("SERVICE_ACCOUNT_FILE_PATH");

        using (HttpClient client = new())
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            using StringContent jsonRequestBody = new(
                JsonSerializer.Serialize(new
                {
                    contents = BuildContent(),
                    systemInstruction = new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = GetEnv("CONTEXT")
                            }
                        }
                    },
                    generationConfig = new
                    {
                        candidateCount = Convert.ToInt32(GetEnv("CANDIDATE_COUNT")),
                        temperature = Convert.ToDouble(GetEnv("TEMPERATURE")),
                        maxOutputTokens = Convert.ToInt32(GetEnv("MAX_OUTPUT_TOKENS")),
                        topP = Convert.ToDouble(GetEnv("TOP-P")),
                        seed = Convert.ToInt32(GetEnv("SEED"))
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_HATE_SPEECH",
                            threshold = "OFF"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                            threshold = "OFF"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                            threshold = "OFF"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_HARASSMENT",
                            threshold = "OFF"
                        }
                    }
                }),
                Encoding.UTF8,
                "application/json");

            string accessToken = GetAccessToken(SERVICE_ACCOUNT_FILE_PATH);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = client.PostAsync(
                $"https://{API_ENDPOINT}/v1/projects/{PROJECT_ID}/locations/{LOCATION_ID}/publishers/{PUBLISHER}/models/{MODEL_ID}:streamGenerateContent",
                jsonRequestBody
            ).Result;

            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);

            // Call recursively to keep on asking for user input prompts
            Main(args);
        }
    }

    private static dynamic BuildContent()
    {
        string jsonContent = File.ReadAllText("contents.json")
            .Replace("{USER_PROMPT}", GetPrompt());

        return JsonSerializer.Deserialize<dynamic[]>(jsonContent);
    }

    private static string GetPrompt()
    {
        Console.WriteLine("Type a prompt:");
        return Console.ReadLine();
    }

    private static string GetAccessToken(string jsonKeyFilePath)
    {
        string SCOPES = GetEnv("SCOPES");
        using var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read);

        return GoogleCredential
            .FromStream(stream)
            .CreateScoped(new string[] { SCOPES })
            .UnderlyingCredential
            .GetAccessTokenForRequestAsync()
            .Result;
    }

    private static string GetEnv(string env)
    {
        return Environment.GetEnvironmentVariable(env);
    }
}