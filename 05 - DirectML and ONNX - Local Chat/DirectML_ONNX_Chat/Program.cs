using DirectML_ONNX_Chat;

var ai = new SLMRunner();

await ai.InitializeAsync();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Type a message to the AI, or press Enter to exit.");
Console.Write("> ");
Console.ForegroundColor = ConsoleColor.White;

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    string? input = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;
    if (String.IsNullOrWhiteSpace(input))
    {
        ai.Dispose();
        return;
    }

    var maxWords = 2000;

    await foreach ( // Generate the response streamingly.
           var text
           in ai.InferStreamingAsync(input))
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(text);

        maxWords--;
        if (maxWords <= 0)
        {
            break;
        }
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("> ");
    Console.ForegroundColor = ConsoleColor.White;
}

