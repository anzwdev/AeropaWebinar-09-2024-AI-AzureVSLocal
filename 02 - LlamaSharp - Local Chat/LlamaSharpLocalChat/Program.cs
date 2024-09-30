using LLama.Common;
using LLama;

//string modelPath = "D:\\Conferences\\2024-09 - Local AI\\00 - Models\\gguff-models\\bartowski-llama-3-groq-8B-tool-use\\Llama-3-Groq-8B-Tool-Use-Q6_K.gguf";
//string modelPath = "D:\\Conferences\\2024-09 - Local AI\\00 - Models\\gguff-models\\teemperror-starcoder2-15b-q6_k\\starcoder2-15b-q6_k.gguf";
//string modelPath = "D:\\Conferences\\2024-09 - Local AI\\00 - Models\\gguff-models\\microsoft-phi-3-4k\\phi-3-4k\\Phi-3-mini-4k-instruct-fp16.gguf";
//string modelPath = "D:\\Conferences\\2024-09 - Local AI\\00 - Models\\gguff-models\\microsoft-phi-3-4k\\Phi-3-mini-4k-instruct-q4.gguf";
string modelPath = "D:\\Conferences\\2024-09 - Local AI\\00 - Models\\gguff-models\\Meta-Llama-3.1-8B-Instruct-Q6_K_L.gguf";

var parameters = new ModelParams(modelPath)
{
    ContextSize = 1024, // The longest length of chat as memory.
    GpuLayerCount = 40 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
};
var model = LLamaWeights.LoadFromFile(parameters);
var context = model.CreateContext(parameters);
var executor = new InteractiveExecutor(context);
var chatHistory = new ChatHistory();

var msg = "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.";

/*
var msg = "Order data extractor in json format from text. You are a converter that reads text from a user ordering items, extract ordr information and returns it in the this json format without asking any additional questions or adding any text before or after json data:\n" +
        "{\n" +
        "  \"customer\": \"customer_name\",\n" +
        "  \"items\": [\n" +
        "    {\n" +
        "      \"name\": \"item name\",\n" +
        "      \"quantity\": \"item quantity\"\n" +
        "    }\n" +
        "  ]\n" +
        "}";
*/
//could you order 7 Dell i7 laptops and 3 HP printers for Cannon Group?

chatHistory.AddMessage(AuthorRole.System, msg);

InferenceParams inferenceParams = new InferenceParams()
{
    MaxTokens = 8000, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
    AntiPrompts = new List<string> { "User:" } // Stop generation once antiprompts appear.
};

Console.ForegroundColor = ConsoleColor.Yellow;
Console.Write("The chat session has started.\nUser: ");
Console.ForegroundColor = ConsoleColor.Green;
var userInput = Console.ReadLine() ?? "";

while (userInput != "exit")
{
    ChatSession session = new(executor, chatHistory);

    await foreach ( // Generate the response streamingly.
        var text
        in session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userInput),
            inferenceParams))
    {
        Console.ForegroundColor = ConsoleColor.White;

        if (text.Trim().EndsWith("User:"))
            Console.Write(text.Trim().Substring(0, text.Trim().Length - 5));
        else
            Console.Write(text);
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
    Console.Write("User: ");
    Console.ForegroundColor = ConsoleColor.Green;
    userInput = Console.ReadLine() ?? "";
}
