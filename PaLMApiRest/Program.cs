// Step 1: Ctrl + Shift + P -> Select .NET: Generate Assets for Build & Debug
// Step 2: Add MODEL and API_KEY env variables

using System.Text;
using System.Text.Json;

string MODEL = Environment.GetEnvironmentVariable("MODEL");
string API_KEY = Environment.GetEnvironmentVariable("API_KEY");

using (HttpClient client = new())
{
    using StringContent jsonRequestBody = new(
        JsonSerializer.Serialize(new
        {
            prompt = new
            {
                text = "Write a story about a treasure hunt"
            },
            temperature = 0.5,
            maxOutputTokens = 800,
            topP = 0.8,
            topK = 40
        }),
        Encoding.UTF8,
        "application/json");

    HttpResponseMessage response = client.PostAsync(
        $"https://generativelanguage.googleapis.com/v1beta2/models/{MODEL}:generateText?key={API_KEY}",
        jsonRequestBody
    ).Result;

    string result = response.Content.ReadAsStringAsync().Result;
    Console.WriteLine(result);
}

