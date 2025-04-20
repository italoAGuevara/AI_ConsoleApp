using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// USING  Function calling
/// </summary>
class Program
{
    private static readonly string apiKey = "";
    private static readonly string endpoint = "https://api.openai.com/v1/responses";
    private static string userInput = "What is the weather like in Bogota today?";
    private static HttpClient httpClient = new HttpClient();
    private static string MODEL = "gpt-4.1-mini";

    static async Task Main(string[] args)
    {
        // Create an HTTP client
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        // Define the request body
        var requestBody = new
        {
            model = MODEL, // ensure the model supports function calls
            input = userInput,            
            tools = new[]
            {
                new
                {

                    type = "function",
                    name = "get_weather",
                    description=  "Get current temperature for a given location.",
                    parameters = new {
                        type = "object",
                        properties = new {
                            location = new {
                                type = "string",
                                description = "City and country e.g. Bogotá, Colombia"
                            }
                        },
                        required = new []{"location" },
                        additionalProperties = false
                    }
                }
            },
            //function_call = "auto" // Let GPT automatically call the function
        };

        // Serialize the request body to JSON
        string jsonString = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        // Send the POST request to OpenAI's API
        var response = await httpClient.PostAsync(endpoint, content);

        // Read the response
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
            if (name == "get_weather")
            {
                // Call your method to get the weather (simulate or call actual API)
                var weatherResult = await GetWeatherAsync(arguments); // Example method
                //Console.WriteLine($"Weather Result: {weatherResult}");



                // Send the response back to OpenAI
                await SendFunctionResponseToOpenAI(weatherResult, call_id);
            }            
        }
        else
        {
            Console.WriteLine("No 'choices' or 'message' found in the response.");
        }
    }

    static async Task<string> GetWeatherAsync(string arguments)
    {
        // Assuming the argument is a JSON string like: { "location": "Paris" }
        var location = JsonSerializer.Deserialize<JsonElement>(arguments).GetProperty("location").GetString();

        // You would call a real weather API here. For now, we're simulating.
        return $"22°C";
    }

    // Sending the result back to OpenAI after calling the function
    static async Task SendFunctionResponseToOpenAI(string functionResult, string call_id)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
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
                            name = "get_weather",
                            arguments = JsonSerializer.Serialize(new { location = "Bogotá, Colombia" })
                        }
                    }
                }
            },
            new {
                role = "tool",
                tool_call_id = call_id,
                name = "get_weather",
                content = functionResult
            }
        };

        var text = new
        {
            format = new
            {
                type = "json_schema",
                name = "weather_answer",
                schema = new
                {
                    type = "object",
                    properties = new
                    {
                        result = "string"
                    }
                }
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

        #region without streaming        
        string followUpJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(followUpJson, Encoding.UTF8, "application/json");        
        var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        //Console.WriteLine("Follow-up Response from OpenAI:");
        Console.WriteLine(responseString);
        using JsonDocument doc = JsonDocument.Parse(responseString);
        //Console.WriteLine(doc.RootElement.ToString());
        //var contentResult = doc.RootElement
        //                  .GetProperty("choices")[0]
        //                  .GetProperty("message")
        //                  .GetProperty("content")
        //                  .GetString();

        //Console.WriteLine(contentResult);
        #endregion
        #region with streaming
        //var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        //// Send the POST request
        //var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        //// Ensure the response was successful
        //response.EnsureSuccessStatusCode();

        //// Stream the response
        //using var stream = await response.Content.ReadAsStreamAsync();
        //using var reader = new System.IO.StreamReader(stream);
        //await Task.Delay(500);  // 500ms delay
        //// Read the response chunk by chunk
        //while (!reader.EndOfStream)
        //{
        //    var line = await reader.ReadLineAsync();
        //    if (!string.IsNullOrEmpty(line))
        //    {
        //        try
        //        {
        //            // Each line is a chunk of the response, so process it
        //            var json = JsonDocument.Parse(line.Replace("data: ",""));
        //            var choices = json.RootElement.GetProperty("choices");

        //            // Extract and display the text output
        //            foreach (var choice in choices.EnumerateArray())
        //            {
        //                var message = choice.GetProperty("delta").GetProperty("content").GetString();
        //                Console.WriteLine(message); // Print the model's output incrementally
        //            }
        //            await Task.Delay(200);  // 500ms delay
        //        }
        //        catch (Exception ex)
        //        {
        //           // Console.WriteLine($"Error processing line: {line}. Exception: {ex.Message}");
        //        }
        //    }
        //}
        #endregion

    }
}





//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// CALL OPENAI USING REST


//class OpenAIChat
//{
//    private static readonly string apiKey = "";
//    private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
//    //https://api.openai.com/v1/responses

//    public static async Task Main()
//    {
//        using var httpClient = new HttpClient();

//        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

//        var movieResultTemplate = new MovieResult()
//        {
//            Movies = new List<Movie>() { 
//                new Movie()
//            {
//                Director = "",
//                IsAvailableOnStreaming = true,
//                Rating = 0,
//                ReleaseYear = 0,
//                Title = ""
//            }}

//        };

//        var jsonMovie = JsonSerializer.Serialize(movieResultTemplate, new JsonSerializerOptions { WriteIndented = true });

//        var requestBody = new
//        {
//            model = "gpt-4",
//            messages = new[]
//            {
//                new { role = "user", content = "Give a list of movies, return the answer using in json format with this structure " + jsonMovie }
//            },
//            temperature = 0.7
//        };

//        string json = JsonSerializer.Serialize(requestBody);
//        var content = new StringContent(json, Encoding.UTF8, "application/json");

//        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
//        string responseJson = await response.Content.ReadAsStringAsync();



//        if (response.IsSuccessStatusCode)
//        {
//            using JsonDocument doc = JsonDocument.Parse(responseJson);
//            Console.WriteLine(doc.RootElement.ToString());
//            var contentResult = doc.RootElement
//                              .GetProperty("choices")[0]
//                              .GetProperty("message")
//                              .GetProperty("content")
//                              .GetString();

//            var match = Regex.Match(contentResult, "```json\\s*(\\{[\\s\\S]*?\\})\\s*```");
//            var stringJsonResult = match.Groups[1].Value;


//            var movieAnwerObject = JsonSerializer.Deserialize<MovieResult>(stringJsonResult);

//            string reply = doc.RootElement
//                              .GetProperty("choices")[0]
//                              .GetProperty("message")
//                              .GetProperty("content")
//                              .GetString();

//            Console.WriteLine("OpenAI: " + reply);
//        }
//        else
//        {
//            Console.WriteLine("Error: " + response.StatusCode);
//            Console.WriteLine(responseJson);
//        }
//    }
//}
