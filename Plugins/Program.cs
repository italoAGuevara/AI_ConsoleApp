using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Plugins;
using System.Text.Json;


#region "OpenAI"

var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
        modelId: "",
        apiKey: "");


//builder.Plugins.AddFromType<MathPlugin>();
builder.Plugins.AddFromObject(new LightsPlugin());
//builder.Plugins.AddFromType<LightsPlugin>();
builder.Plugins.AddFromObject(new MathPlugin());

var kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var executionSettings = new OpenAIPromptExecutionSettings()
{
    //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),    
    ResponseFormat = typeof(MathResponse), // Specify response format class,
    Temperature = 0.1,
    MaxTokens = 100,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//var result = await kernel.InvokePromptAsync("What are the top 10 movies of all time?", new(executionSettings));
//var result = await kernel.InvokePromptAsync("Multiply 2 and 50?");
//Console.WriteLine(result);


var history = new ChatHistory();
//history.AddUserMessage("Give me the state of the lights");
//history.AddUserMessage("Give me the state of the light 2 and 3");
//history.AddUserMessage("turn off the light");
//history.AddUserMessage("Change the state of the light Chandelier, set brightness to 50");
history.AddUserMessage("What is 2 multiply 3");


var result = await chatCompletionService.GetChatMessageContentAsync(
history,
  executionSettings: executionSettings,
  kernel: kernel);

Console.WriteLine(result.Content);
//var movieResult = JsonSerializer.Deserialize<MovieResult>(result.ToString());

//// Output the result
//for (var i = 0; i < movieResult.Movies.Count; i++)
//{
//    var movie = movieResult.Movies[i];

//    Console.WriteLine($"Movie #{i + 1}");
//    Console.WriteLine($"Title: {movie.Title}");
//    Console.WriteLine($"Director: {movie.Director}");
//    Console.WriteLine($"Release year: {movie.ReleaseYear}");
//    Console.WriteLine($"Rating: {movie.Rating}");
//    Console.WriteLine($"Is available on streaming: {movie.IsAvailableOnStreaming}");
//    Console.WriteLine($"Tags: {string.Join(",", movie.Tags)}");
//}

#endregion

#region "OLLAMA"
/***************************************************************************************************************************************/
//var URI = new Uri("http://localhost:11434");
//string MODEL = "llama3.2";
/*OLLAMA*/
//#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//var builder = Kernel.CreateBuilder().AddOllamaChatCompletion(new OllamaApiClient(URI, MODEL));
//#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//builder.Plugins.AddFromType<MathPlugin>();
//builder.Plugins.AddFromObject(new LightsPlugin());
//builder.Plugins.AddFromType<LightsPlugin>();
//builder.Plugins.AddFromObject(new MathPlugin());
//builder.Plugins.AddFromFunctions("time_plugin",
//[
//    KernelFunctionFactory.CreateFromMethod(
//        method: () => DateTime.Now,
//        functionName: "get_time",
//        description: "Get the current time"
//    ),
//    KernelFunctionFactory.CreateFromMethod(
//        method: (DateTime start, DateTime end) => (end - start).TotalSeconds,
//        functionName: "diff_time",
//        description: "Get the difference between two times in seconds"
//    ),
//    KernelFunctionFactory.CreateFromMethod(
//        method: (int number1, int number2) => (number1 * number2),
//        functionName: "multiply",
//        description: "Multiply 2 numbers"
//    ),
//    KernelFunctionFactory.CreateFromMethod(
//        method: () => new string[] { "Volvo", "BMW", "Ford", "Mazda" },
//        functionName: "list_of_cars",
//        description: "return the list of cars on the system"
//    ),
//]);
//var kernel = builder.Build();

//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();



//#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//var settings = new OllamaPromptExecutionSettings()
//{
//    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
//};


//#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Create a history store the conversation
//var history = new ChatHistory();
//history.AddUserMessage("Give me the state of the lights");
//history.AddUserMessage("Give me the state of the light 2 and 3");
//history.AddUserMessage("turn off the light");
//history.AddUserMessage("Change the state of the light Chandelier, set brightness to 50");
//history.AddUserMessage("What is 2 multiply 3");

// Get the response from the AI
//var result = await chatCompletionService.GetChatMessageContentAsync(
//history,
//   executionSettings: settings,
//   kernel: kernel);

// Print the results
//Console.WriteLine("Assistant > " + result);

#endregion