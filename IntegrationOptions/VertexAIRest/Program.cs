// Step 1: dotnet add package Google.Apis.Auth --version 1.63.0
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
                    instances = new[] { new { content = GetContentValue() } },
                    parameters = new
                    {
                        candidateCount = Convert.ToInt32(GetEnv("CANDIDATE_COUNT")),
                        maxOutputTokens = Convert.ToInt32(GetEnv("MAX_OUTPUT_TOKENS")),
                        temperature = Convert.ToDouble(GetEnv("TEMPERATURE")),
                        topP = Convert.ToDouble(GetEnv("TOP-P")),
                        topK = Convert.ToInt32(GetEnv("TOP-K"))
                    }
                }),
                Encoding.UTF8,
                "application/json");

            string accessToken = GetAccessToken(SERVICE_ACCOUNT_FILE_PATH);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = client.PostAsync(
                $"https://{API_ENDPOINT}/v1/projects/{PROJECT_ID}/locations/{LOCATION_ID}/publishers/{PUBLISHER}/models/{MODEL_ID}:predict",
                jsonRequestBody
            ).Result;

            string result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);

            // Call recursively to keep on asking for user input prompts
            Main(args);
        }        
    }

    private static string GetPrompt()
    {
        Console.WriteLine("Type a prompt:");
        return Console.ReadLine();
    }

    private static string GetContentValue()
    {
        try
        {
            using StreamReader reader = new("examples.json");
            JsonDocument jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
            JsonElement root = jsonDocument.RootElement;
            StringBuilder inputOutputInstances = new();

            foreach (JsonElement example in root.EnumerateArray())
            {
                string inputValue = example.GetProperty("input").GetString();
                string outputValue = example.GetProperty("output").GetString();
                inputOutputInstances.Append($"input: {inputValue}\noutput: {outputValue}\n\n");
            }
            string context = GetEnv("CONTEXT");
            string contentValue = $"{context}\n\n{inputOutputInstances}input: {GetPrompt()}\noutput:\n";
            
            // TODO: return in line after seeing contentValue in debug -> Copy value into text file
            return contentValue; 
        }
        catch (IOException ex)
        {
            throw new IOException("Resource not found: examples.json", ex);
        }
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