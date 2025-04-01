// Instalación de la biblioteca .NET a través de NuGet: dotnet add package Azure.AI.OpenAI --prerelease
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

using static System.Environment;
using System.Text.Json;
using Azure_project;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

async Task RunAsync()
{
    // Recuperación del punto de conexión de OpenAI a partir de las variables de entorno
    var endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "";
    if (string.IsNullOrEmpty(endpoint))
    {
        Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
        return;
    }

    var key = "";
    if (string.IsNullOrEmpty(key))
    {
        Console.WriteLine("Please set the AZURE_OPENAI_KEY environment variable.");
        return;
    }

    AzureKeyCredential credential = new AzureKeyCredential(key);

    // Inicialización de AzureOpenAIClient
    AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);

    // Inicializar ChatClient con el nombre de implementación especificado
    ChatClient chatClient = azureClient.GetChatClient("gpt-4o-mini");

    // Crear una lista de mensajes de chat
    var messages = new List<ChatMessage>
    {
    };

    messages.Add(new UserChatMessage("Give me some movies examples"));

    // Crear opciones de finalización de chat
    ChatResponseFormat chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
jsonSchemaFormatName: "movie_result",
jsonSchema: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "Movies": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "Title": { "type": "string" },
                            "Director": { "type": "string" },
                            "ReleaseYear": { "type": "integer" },
                            "Rating": { "type": "number" },
                            "IsAvailableOnStreaming": { "type": "boolean" },
                            "Tags": { "type": "array", "items": { "type": "string" } }
                        },
                        "required": ["Title", "Director", "ReleaseYear", "Rating", "IsAvailableOnStreaming", "Tags"],
                        "additionalProperties": false
                    }
                }
            },
            "required": ["Movies"],
            "additionalProperties": false
        }
        """),
jsonSchemaIsStrict: true);



    var options = new ChatCompletionOptions
    {
        Temperature = (float)0.7,
        MaxOutputTokenCount = 800,

        TopP = (float)0.95,
        FrequencyPenalty = (float)0,
        PresencePenalty = (float)0,
        ResponseFormat = chatResponseFormat
    };

   


    try
    {
        // Crear la solicitud de finalización del chat
        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

        // Imprimir la respuesta
        if (completion != null)
        {
            Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));

            Console.WriteLine("-------------------------------------------------------------------------\n");
            Console.WriteLine(completion.Content.First().Text);
        }
        else
        {
            Console.WriteLine("No response received.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

await RunAsync();