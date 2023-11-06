// Step 1: dotnet add package Google.Apis.Auth --version 1.63.0
// Step 2: Ctrl + Shift + P -> Select .NET: Generate Assets for Build & Debug
// Step 3: Add API_ENDPOINT, PROJECT_ID, and MODEL env variables

using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;

string API_ENDPOINT = Environment.GetEnvironmentVariable("API_ENDPOINT");
string PROJECT_ID = Environment.GetEnvironmentVariable("PROJECT_ID");
string MODEL = Environment.GetEnvironmentVariable("MODEL");
string SERVICE_ACCOUNT_FILE_PATH = Environment.GetEnvironmentVariable("SERVICE_ACCOUNT_FILE_PATH");

using (HttpClient client = new())
{
    using StringContent jsonRequestBody = new(
        JsonSerializer.Serialize(new
        {
            instances = new
            {
                content = "Write a story about a treasure hunt"
            },
            parameters = new
            {
                candidateCount = 1,
                maxOutputTokens = 1024,
                temperature = 0.5,
                topP = 0.8,
                topK = 40
            }
        }),
        Encoding.UTF8,
        "application/json");

    string accessToken = GetAccessToken(SERVICE_ACCOUNT_FILE_PATH);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    HttpResponseMessage response = client.PostAsync(
        $"https://{API_ENDPOINT}/v1/projects/{PROJECT_ID}/locations/us-central1/publishers/google/models/{MODEL}:predict",
        jsonRequestBody
    ).Result;

    string result = response.Content.ReadAsStringAsync().Result;
    Console.WriteLine(result);
}

static string GetAccessToken(string jsonKeyFilePath)
{
    string SCOPES = Environment.GetEnvironmentVariable("SCOPES");
    using var stream = new FileStream(jsonKeyFilePath, FileMode.Open, FileAccess.Read);

    return GoogleCredential
        .FromStream(stream)
        .CreateScoped(new string[] { SCOPES })
        .UnderlyingCredential
        .GetAccessTokenForRequestAsync()
        .Result;
}

