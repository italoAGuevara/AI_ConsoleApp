using Microsoft.SemanticKernel;
using System.ComponentModel;

public class MathPlugin
{
    [KernelFunction, Description("Add 2 numbers")]
    public int Add(int a, int b)
    {
        Console.WriteLine("Sumando 2 numeros");
        return a + b;
    }

    [KernelFunction, Description("Multiply 2 numbers")]
    public int Multiply(int a, int b)
    {
        Console.WriteLine("Multipliyng");
        return a * b;
    }
}

public class MathResponse
{
    public int result {  get; set; }
    public string message { get; set; }
}

