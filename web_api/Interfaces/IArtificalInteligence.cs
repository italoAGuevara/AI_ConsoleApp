namespace web_api.Interfaces
{
    public interface IArtificalInteligence
    {
        Task<object> executeFunctionCaller(string userInput);
    }
}
