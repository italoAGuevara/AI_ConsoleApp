using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

class Program
{
    static readonly string openAiKey = "";
    static readonly string openAiEndpoint = "";
    static readonly string openAiEmbeddingDeployment = "";
    static readonly string openAiChatDeployment = "";

    static readonly string searchServiceName = "";
    static readonly string searchIndexName = "";
    static readonly string searchKey = "";

    static async Task Main(string[] args)
    {
        string userQuery = "Quien es el autor?";

        // 1. Obtener embedding del query
        var queryEmbedding = await GetEmbeddingAsync(userQuery);

        // 2. Buscar documentos similares en Azure Cognitive Search
        var searchResults = await SearchSimilarDocumentsAsync(queryEmbedding);

        // 3. Construir prompt con los documentos
        string context = string.Join("\n---\n", searchResults);
        string prompt = $"Usa el siguiente contexto para responder la pregunta:\n\n{context}\n\nPregunta: {userQuery}";

        // 4. Enviar prompt al modelo GPT
        var answer = await GetChatCompletionAsync(prompt);

        Console.WriteLine("\n🧠 Respuesta:");
        Console.WriteLine(answer);
    }

    static async Task<List<float>> GetEmbeddingAsync(string input)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);

        var payload = new { input = input, model = openAiEmbeddingDeployment };
        var response = await client.PostAsJsonAsync("", payload);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var values = doc.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToList();
        return values;
    }

    static async Task<List<string>> SearchSimilarDocumentsAsync(List<float> embedding)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", searchKey);

        //var body = new
        //{
        //    vector = new
        //    {
        //        value = embedding,
        //        k = 3,
        //        fields = "embedding"
        //    }
        //};
        var body = new
        {
            search = "*",  // Match all documents
            top = 3         // Limit the number of results
        };


        var response = await client.PostAsJsonAsync(
            $"{searchIndexName}/docs/search?api-version=2023-07-01-Preview",
            body);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var docs = new List<string>();
        foreach (var result in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            if (result.TryGetProperty("content", out var contentElement))
                docs.Add(contentElement.GetString());
        }

        return docs;
    }

    static async Task<string> GetChatCompletionAsync(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);

        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = "Eres un asistente útil." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        var response = await client.PostAsJsonAsync(
            $"",
            payload);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}
