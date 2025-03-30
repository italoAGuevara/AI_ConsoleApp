

using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Text;
using System.Text.Json;

var URI = new Uri("http://localhost:11434");
string MODEL = "llama3.2";
int MAX_TOKENS = 50; // Limit response to 500 tokens
float Temperature = .1f;

// Initialize ChatResponseFormat object with JSON schema of desired response format.
// Define a JSON schema as a string
        string jsonSchema = @"
        {
            ""type"": ""object"",
            ""properties"": {
                ""Movies"": { 
                    ""type"": ""array"",
                    ""items"":{
                        ""type"": ""object"",
                        ""properties"": {
                            ""Title"": { ""type"": ""string"" },
                            ""Director"": { ""type"": ""string"" },
                            ""ReleaseYear"": { ""type"": ""integer"" },
                            ""Rating"": { ""type"": ""number"" },
                            ""IsAvailableOnStreaming"": { ""type"": ""boolean"" },
                            ""Tags"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } }
                        },
                        ""required"": [""Title"", ""Director"", ""ReleaseYear"", ""Rating"", ""IsAvailableOnStreaming"", ""Tags""],
                        ""additionalProperties"": false
                }
                
            },
            ""required"": [""name"", ""age""]
        }";

// Parse JSON schema into a JsonElement
using JsonDocument doc = JsonDocument.Parse(jsonSchema);
JsonElement schemaElement = doc.RootElement;
ChatResponseFormat chatResponseFormat = ChatResponseFormat.ForJsonSchema(schemaElement, "movie_result", "this is a resulft schema wich has movies");

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var settings = new OllamaPromptExecutionSettings()
{
    Temperature = Temperature,
    NumPredict = MAX_TOKENS,
    ExtensionData = Dictionary<string, ChatResponseFormat>
};
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var kernel = Kernel.CreateBuilder().AddOllamaChatCompletion(new OllamaApiClient(URI, MODEL)).Build();
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


var functionResult = await kernel.InvokePromptAsync(chatPrompt);


var messageContent = functionResult.GetValue<ChatMessageContent>(); // Retrieves underlying chat message content from FunctionResult.
var replyInnerContent = messageContent!.InnerContent as ChatDoneResponseStream;

Console.WriteLine(messageContent);
Console.ReadLine();