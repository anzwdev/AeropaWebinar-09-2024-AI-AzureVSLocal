using DirectML_ONNX_Search;

var index = new DirectML_ONNX_Search.Index();

Console.WriteLine("Initializing the AI model...");
await index.InitializeSMLRunnerAsync();

Console.WriteLine("Indexing the documents...");
await index.IndexFolder("D:\\Conferences\\2024-09 - Local AI\\00 - Microsoft Docs Files");

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Type a question to the AI, or press Enter to exit.");
Console.Write("> ");
Console.ForegroundColor = ConsoleColor.White;

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    string? input = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;

    if (String.IsNullOrWhiteSpace(input))
    {        
        return;
    }

    //await index.SearchNoChatAI(input);
    await index.Search(input);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("> ");
    Console.ForegroundColor = ConsoleColor.White;
}

