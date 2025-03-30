using AI_ConsoleApp_Ollama_RAG;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;


var movieData = new List<PNR>()
{
    
    new PNR
        {
            Key=2,
            CreatredBy = "Italo",
            Record = "PLF513",
            Index = 1
        },
    new PNR
        {
            Key=3,
            CreatredBy = "Juanpa",
            Record = "D5H9J3",
            Index = 2
        },
        new PNR
        {
            Key=1,
            CreatredBy = "Italo",
            Record = "ABC123",
            Index = 23
        },
    new PNR
        {
            Key=4,
            CreatredBy = "Emanuel",
            Record = "26TH82",
            Index=7
        },

};

var vectorStore = new InMemoryVectorStore();

var movies = vectorStore.GetCollection<int, PNR>("PNR");

await movies.CreateCollectionIfNotExistsAsync();

IEmbeddingGenerator<string, Embedding<float>> generator = new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "all-minilm");

foreach (var movie in movieData)
{
    movie.Vector = await generator.GenerateEmbeddingVectorAsync(movie.Record);
    movie.Vector = await generator.GenerateEmbeddingVectorAsync(movie.CreatredBy);
    await movies.UpsertAsync(movie);
}

var query = "Give all PNR ";
var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);

var searchOptions = new VectorSearchOptions<PNR>()
{
    Top = 1,
    //VectorPropertyName = "Vector"
    IncludeTotalCount = true,
    VectorPropertyName = "Vector",
    
};


var results = await movies.VectorizedSearchAsync(queryEmbedding, searchOptions);

await foreach (var result in results.Results)
{
    Console.WriteLine($"Title: {result.Record.Record}");
    Console.WriteLine($"Created by: {result.Record.CreatredBy}");    
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine();
}