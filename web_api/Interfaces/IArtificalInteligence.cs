namespace web_api.Interfaces
{
    public interface IArtificalInteligence
    {
        Task<object> executeFunctionCaller(string userInput);
        Task<object> executeTextEmbeddings(string userInput);
        Task<object> executeRAG(string userInput);
    }
}
