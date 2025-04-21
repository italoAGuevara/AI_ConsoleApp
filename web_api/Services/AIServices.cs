using System.Reflection;
using System.Text.Json;
using System.Text;
using web_api.Interfaces;
using web_api.Controllers;
using OpenAI.Chat;
using OpenAI.Assistants;
using OpenAI.Embeddings;
using OpenAI.Files;
using OpenAI;
using System.ClientModel;

namespace web_api.Services
{
    public class AIServices : IArtificalInteligence
    {
        private static readonly string apiKey = "";
        private static readonly string endpoint = "https://api.openai.com/v1/responses";        
        private static HttpClient httpClient = new HttpClient();
        private static string MODEL = "gpt-4.1-mini";
        private readonly WeatherForecastController _weatherServices;
        ChatClient client = new(
          model: MODEL,
          apiKey
        );

        object function_get_weather =   new
        {
            type = "function",
            name = "get_list_of_weathers",
            description=  "return a object list of weather explation, the information include name of weather, date and temperature",
            function_call = "auto"
        };

        public AIServices(WeatherForecastController weatherServices)
        {
            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _weatherServices = weatherServices;
        }
        

        public async Task<object> executeFunctionCaller(string userInput)
        {
            var result = call(userInput);
            return result;
        }

        public async Task<object> executeTextEmbeddings(string userInput )
        {
            EmbeddingClient client = new("text-embedding-3-small", apiKey);

            string description = "Best hotel in town if you like luxury hotels. They have an amazing infinity pool, a spa,"
                + " and a really helpful concierge. The location is perfect -- right downtown, close to all the tourist"
                + " attractions. We highly recommend this hotel.";
            EmbeddingGenerationOptions options = new() { Dimensions = 512 };

            OpenAIEmbedding embedding = client.GenerateEmbedding(description, options);            
            ReadOnlyMemory<float> vector = embedding.ToFloats();
            return vector;
        }

        public async Task<object> executeRAG(string userInput)
        {
            OpenAIClient openAIClient = new(apiKey);
            OpenAIFileClient fileClient = openAIClient.GetOpenAIFileClient();
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            AssistantClient assistantClient = openAIClient.GetAssistantClient();

            using Stream document = BinaryData.FromBytes("""
            {
                "description": "This document contains the sale history data for Contoso products.",
                "sales": [
                    {
                        "month": "January",
                        "by_product": {
                            "113043": 15,
                            "113045": 12,
                            "113049": 2
                        }
                    },
                    {
                        "month": "February",
                        "by_product": {
                            "113045": 22
                        }
                    },
                    {
                        "month": "March",
                        "by_product": {
                            "113045": 16,
                            "113055": 5
                        }
                    }
                ]
            }
            """u8.ToArray()).ToStream();
            OpenAIFile salesFile = fileClient.UploadFile(
                document,
                "monthly_sales.json",
                FileUploadPurpose.Assistants);


            AssistantCreationOptions assistantOptions = new()
            {
                Name = "Example: Contoso sales RAG",
                Instructions =
                "You are an assistant that looks up sales data and helps visualize the information based"
                + " on user queries. When asked to generate a graph, chart, or other visualization, use"
                + " the code interpreter tool to do so.",
                        Tools =
            {
                new FileSearchToolDefinition(),
                new CodeInterpreterToolDefinition(),
            },
                        ToolResources = new()
                        {
                            FileSearch = new()
                            {
                                NewVectorStores =
                    {
                        new VectorStoreCreationHelper([salesFile.Id]),
                    }
                            }
                        },
                    };
            Assistant assistant = assistantClient.CreateAssistant("gpt-4o", assistantOptions);

            ThreadCreationOptions threadOptions = new()
            {
                InitialMessages = { "How well did product 113045 sell in February? Graph its trend over time." }
            };

            ThreadRun threadRun = assistantClient.CreateThreadAndRun(assistant.Id, threadOptions);

            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                threadRun = assistantClient.GetRun(threadRun.ThreadId, threadRun.Id);
            } while (!threadRun.Status.IsTerminal);

            CollectionResult<ThreadMessage> messages
    = assistantClient.GetMessages(threadRun.ThreadId, new MessageCollectionOptions() { Order = MessageCollectionOrder.Ascending });

            foreach (ThreadMessage message in messages)
            {
                Console.Write($"[{message.Role.ToString().ToUpper()}]: ");
                foreach (MessageContent contentItem in message.Content)
                {
                    if (!string.IsNullOrEmpty(contentItem.Text))
                    {
                        Console.WriteLine($"{contentItem.Text}");

                        if (contentItem.TextAnnotations.Count > 0)
                        {
                            Console.WriteLine();
                        }

                        // Include annotations, if any.
                        foreach (TextAnnotation annotation in contentItem.TextAnnotations)
                        {
                            if (!string.IsNullOrEmpty(annotation.InputFileId))
                            {
                                Console.WriteLine($"* File citation, file ID: {annotation.InputFileId}");
                            }
                            if (!string.IsNullOrEmpty(annotation.OutputFileId))
                            {
                                Console.WriteLine($"* File output, new file ID: {annotation.OutputFileId}");
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(contentItem.ImageFileId))
                    {
                        OpenAIFile imageInfo = fileClient.GetFile(contentItem.ImageFileId);
                        BinaryData imageBytes = fileClient.DownloadFile(contentItem.ImageFileId);
                        using FileStream stream = File.OpenWrite($"{imageInfo.Filename}.png");
                        imageBytes.ToStream().CopyTo(stream);

                        Console.WriteLine($"<image: {imageInfo.Filename}.png>");
                    }
                }
                Console.WriteLine();
            }
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            return null;
        }

        private async Task<object> call(string userInput)
        {
            var getListOfWeathersTool = ChatTool.CreateFunctionTool(
                functionName : "get_list_of_weathers",
                functionDescription : "return a object list of weather explation, the information include name of weather, date and temperature"
            );

            var getUserByIdTool = ChatTool.CreateFunctionTool(
                functionName: "get_user_by_id",
                functionDescription: "return user information, search by id",
                functionParameters : BinaryData.FromBytes("""
                    {
                        "type": "object",
                        "properties": {
                            "id": {
                                "type": "number",
                                "description": "id of the user"
                            }
                        },
                        "required": [ "id" ]
                    }
                    """u8.ToArray())
            );

            List<ChatMessage> messages =
            [
                new UserChatMessage(userInput),
            ];

            ChatCompletionOptions options = new()
            {
                Tools = { getListOfWeathersTool, getUserByIdTool },
            };
                        
            ChatCompletion completion = client.CompleteChat(messages, options);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                    {
                        messages.Add(new AssistantChatMessage(completion));
                        break;
                    }

                case ChatFinishReason.ToolCalls:
                    {
                        messages.Add(new AssistantChatMessage(completion));

                        foreach (ChatToolCall toolCall in completion.ToolCalls)
                        {
                            switch (toolCall.FunctionName)
                            {
                                case "get_list_of_weathers":
                                    {     
                                        var weatherResult = _weatherServices.get_list_of_weathers();
                                        var jsonList = JsonSerializer.Serialize(weatherResult);
                                        string toolResult = jsonList;
                                        messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                        break;
                                    }
                                case "get_user_by_id":
                                    {
                                        using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                        bool hasLocation = argumentsJson.RootElement.TryGetProperty("id", out JsonElement idAsString);
                                        int id = int.Parse(idAsString.ToString());
                                        var user = new User().GetUsers().FirstOrDefault(x => x.Id == id);
                                        var jsonUser = JsonSerializer.Serialize(user);
                                        string toolResult = jsonUser;
                                        messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                        break;
                                    }

                                default:
                                    {
                                        // Handle other unexpected calls.
                                        throw new NotImplementedException();
                                    }
                            }
                        }

                        break;
                    }

                case ChatFinishReason.Length:
                    throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                case ChatFinishReason.ContentFilter:
                    throw new NotImplementedException("Omitted content due to a content filter flag.");

                case ChatFinishReason.FunctionCall:
                    throw new NotImplementedException("Deprecated in favor of tool calls.");

                default:
                    throw new NotImplementedException(completion.FinishReason.ToString());
            }

            return messages;
        }

        [Obsolete]
        private async Task<object> firstCall(string userInput)
        {
            var requestBody = new
            {
                model = MODEL,
                input = userInput,
                temperature = 0,
                top_p = 0.1,
                tools = new[]
                {
                    function_get_weather
                },
            };

            string jsonString = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");            
            var response = await httpClient.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

            // Navigate to the first item in the "output" array
            JsonElement outputItem = jsonResponse.GetProperty("output")[0];

            // Extract "arguments" and "name" as strings
            string arguments = outputItem.GetProperty("arguments").GetString();
            string name = outputItem.GetProperty("name").GetString();
            string call_id = outputItem.GetProperty("call_id").GetString();

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(arguments))
            {
                // Call the appropriate method based on the function name
                if (name == "get_list_of_weathers")
                {
                    // Call your method to get the weather (simulate or call actual API)
                    var weatherResult = _weatherServices.get_list_of_weathers();
                    var jsonList = JsonSerializer.Serialize(weatherResult);
                    responseString = await SendFunctionResponseToOpenAI(userInput , jsonList, call_id);
                }
            }

            return responseString;
        }

        [Obsolete]
        private async Task<string> SendFunctionResponseToOpenAI(string userInput,string functionResult, string call_id)
        {
            var input = new object[]
            {
                new {
                    role = "user",
                    content = userInput
                },
                new {
                    role = "assistant",
                    content = "",
                    tool_calls = new[] {
                        new {
                            id = call_id,
                            type = "function",
                            function = new {
                                name = "get_list_of_weathers",
                                arguments = "{}"
                            }
                        }
                    }
                },
                new {
                    role = "tool",
                    tool_call_id = call_id,
                    name = "get_list_of_weathers",
                    content = functionResult
                }
            };


            var requestBody = new
            {
                model = MODEL,
                messages = input,
                temperature = 0,
                top_p = 0.1
                //stream = true
            };
 
            string followUpJson = JsonSerializer.Serialize(requestBody);
            var contentPetition = new StringContent(followUpJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", contentPetition);
            var responseString = await response.Content.ReadAsStringAsync();
                        
            return responseString;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int age { get; set; }

        public List<User> GetUsers()
        {
            var users = new List<User>();

            users.Add(new User { Id = 1,firstName = "italo", lastName = "guevara", age = 28 });
            users.Add(new User { Id = 2, firstName = "cesar", lastName = "parra", age = 53 });
            users.Add(new User { Id = 3, firstName = "olga", lastName = "villamil", age = 28 });
            users.Add(new User { Id = 4, firstName = "luisa", lastName = "beltran", age = 18 });

            return users;
        }
        
    }
}
