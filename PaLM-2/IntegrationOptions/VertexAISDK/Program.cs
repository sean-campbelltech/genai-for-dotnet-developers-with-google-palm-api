// Step 1: dotnet add package Google.Cloud.AIPlatform.V1 --version 2.20.0
// Step 2: dotnet add package Google.Apis.Auth --version 1.63.0
// Step 3: Add environment variable in launch.json file as per https://github.com/googleapis/google-cloud-dotnet#authentication
//  - Step 3.1: Ctrl + Shift + P: Select .NET: Generate Assets for Build and Debug
//  - Step 3.2: Add GOOGLE_APPLICATION_CREDENTIALS in launch.json file as env variable for Google Credentials
// Step 4: Add env variables
// Step 5: Change "console": "integratedTerminal" in launch.json 

using System.Globalization;
using System.Text;
using System.Text.Json;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;

internal class Program
{
    private static string LOCATION_ID = GetEnv("LOCATION_ID");
    
    private static void Main(string[] args)
    {
        List<Google.Protobuf.WellKnownTypes.Value> instances = GetInstances();
        Google.Protobuf.WellKnownTypes.Value parameters = GetParameters();

        PredictTextPrompt(instances, parameters);        
        Main(args);
    }

    private static void PredictTextPrompt(IEnumerable<Google.Protobuf.WellKnownTypes.Value> instances, Google.Protobuf.WellKnownTypes.Value parameters)
    {
        string PROJECT = GetEnv("PROJECT");
        string MODEL_ID = GetEnv("MODEL_ID");
        string PUBLISHER = GetEnv("PUBLISHER");

        var serviceClientBuilder = new PredictionServiceClientBuilder
        {
            Endpoint = $"{LOCATION_ID}-aiplatform.googleapis.com:443"
        };

        PredictionServiceClient predictionServiceClient = serviceClientBuilder.Build();
        EndpointName endpoint = EndpointName.FromProjectLocationPublisherModel(PROJECT, LOCATION_ID, PUBLISHER, MODEL_ID);
        PredictResponse response = predictionServiceClient.Predict(endpoint, instances, parameters);

        Console.WriteLine(response.Predictions[0].StructValue.Fields["content"]);
    }

    private static Google.Protobuf.WellKnownTypes.Value GetParameters()
    {   
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        return new()
        {
            StructValue = Struct.Parser.ParseJson(
                JsonSerializer.Serialize(new
                    {
                        candidateCount = Convert.ToInt32(GetEnv("CANDIDATE_COUNT")),
                        maxOutputTokens = Convert.ToInt32(GetEnv("MAX_OUTPUT_TOKENS")),
                        temperature = Convert.ToDouble(GetEnv("TEMPERATURE")),
                        topP = Convert.ToDouble(GetEnv("TOP-P")),
                        topK = Convert.ToInt32(GetEnv("TOP-K"))
                    })
            )
        };
    }

    private static List<Google.Protobuf.WellKnownTypes.Value> GetInstances()
    {
        return new()
        {
            new Google.Protobuf.WellKnownTypes.Value
            {
                StructValue = Struct.Parser.ParseJson(
                    JsonSerializer.Serialize(new { content = GetContentValue() }
                ))
            }
        };
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

    private static string GetEnv(string env)
    {
        return Environment.GetEnvironmentVariable(env);
    }
}