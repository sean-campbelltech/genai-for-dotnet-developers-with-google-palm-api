// Step 1: dotnet add package Google.Cloud.AIPlatform.V1 --version 2.20.0
// Step 2: dotnet add package Google.Apis.Auth --version 1.63.0
// Step 3: Add environment variable in launch.json file as per https://github.com/googleapis/google-cloud-dotnet#authentication
//  - Step 3.1: Ctrl + Shift + P: Select .NET: Generate Assets for Build and Debug
//  - Step 3.2: Add GOOGLE_APPLICATION_CREDENTIALS in launch.json file as env variable
// Step 4: Add PROJECT, LOCATION, MODEL, PUBLISHER env 
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;

List<Google.Protobuf.WellKnownTypes.Value> instances = new()
{
    new Google.Protobuf.WellKnownTypes.Value
    {
        StructValue = Struct.Parser.ParseJson("{ \"prompt\": \"Write a story about a treasure hunt\" }")
    }
};
Google.Protobuf.WellKnownTypes.Value parameters = new()
{
    StructValue = Struct.Parser.ParseJson("{ " +
        "\"temperature\": 0.2, " +
        "\"maxOutputTokens\": 1024, " +
        "\"topP\": 0.8, " +
        "\"topK\": 40 " +
    "}")
};
PredictTextPrompt(instances, parameters);

static void PredictTextPrompt(IEnumerable<Google.Protobuf.WellKnownTypes.Value> instances, Google.Protobuf.WellKnownTypes.Value parameters)
{
    string PROJECT = Environment.GetEnvironmentVariable("PROJECT");
    string LOCATION = Environment.GetEnvironmentVariable("LOCATION");
    string MODEL = Environment.GetEnvironmentVariable("MODEL");
    string PUBLISHER = Environment.GetEnvironmentVariable("PUBLISHER");
    string REGION_ENDPOINT = Environment.GetEnvironmentVariable("REGION_ENDPOINT");

    var serviceClientBuilder = new PredictionServiceClientBuilder
    {
        Endpoint = REGION_ENDPOINT
    };

    PredictionServiceClient predictionServiceClient = serviceClientBuilder.Build();
    EndpointName endpoint = EndpointName.FromProjectLocationPublisherModel(PROJECT, LOCATION, PUBLISHER, MODEL);
    PredictResponse response = predictionServiceClient.Predict(endpoint, instances, parameters);

    Console.WriteLine(response.Predictions.ToString());
}
