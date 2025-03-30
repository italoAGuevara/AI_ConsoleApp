using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using Microsoft.SemanticKernel.Connectors.Ollama;

class Program
{
    static async Task Main()
    {
        var URI = new Uri("http://localhost:11434");
        string MODEL = "llama3.2";
        int MAX_TOKENS = 500; // Limit response to 500 tokens

        // Set up Dependency Injection (DI) container
        var serviceProvider = new ServiceCollection()
            .AddSingleton<OllamaApiClient>(sp => new OllamaApiClient(URI, MODEL))
            .BuildServiceProvider();

        // Get Ollama API client from service provider
        var ollama = serviceProvider.GetRequiredService<OllamaApiClient>();

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var settings = new OllamaPromptExecutionSettings() 
        {
            Temperature = 0,
            NumPredict = 50,            
        };
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        // Create chat completion service
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var chatService = ollama.AsChatCompletionService();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        
        // Initialize chat history
        var chatHistory = new ChatHistory("You are futbol soccer assistan");
        
        while (true)
        {
            Console.Write("\n--------------- ");
            Console.WriteLine("User question : ");
            var input = Console.ReadLine();
            
            chatHistory.AddUserMessage(input);

            // Get assistant response
            var reply = await chatService.GetChatMessageContentAsync(chatHistory, settings);

            
            // Add the message from the agent to the chat history
            chatHistory.AddSystemMessage(reply.Content);
            chatHistory.AddDeveloperMessage("You are futbol soccer assistan");

            Console.Write("\n--------------- ");
            Console.Write("Assistant: ");
            Console.WriteLine(reply.Content);
            Console.Write("\n---------------");
        }        
    }
}
