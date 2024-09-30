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

var msg =
    "You are a Microsoft Business Central AL developer assistant and help developers adding helpful tooltips to their table definitions. " +
    "For each table and list of fields, you will produce list of fields and their descriptive tooltips that help users understand meaning of the field and return it in this json format whithout adding any questions or texts so your response is always a valid json: " +
    "{\n"+
    "  \"table\": \"<table_name>\",\\n\"+" +
    "  \"fields\":\\n\"+" +
    "    [\\n\"+" +
    "      {\\n\"+" +
    "        \"name\": \"<field_name>\",\\n\"+" +
    "        \"tooltip\": \"<field_tooltip>\"\\n\"+" +
    "      },\\n\"+" +
    "      {\\n\"+" +
    "        \"name\":\\n\"+" +
    "        \"<field_name>\", \"tooltip\": \"<field_tooltip>\"\\n\"+" +
    "      }\\n\"+" +
    "    ]\\n\"+" +
    "}\n";

chatHistory.AddMessage(AuthorRole.System, msg);

var userMessage = // "Add missing tooltips properties to fields in this Business Central table and return updated source code:\n" +
    "table \"Car\", fields: " +
    "\"No.\", " +
    "\"Brand Code\", " +
    "\"Model Code\", " +
    "\"Engine Type\", " +
    "\"Engine Size\", " +
    "\"Registration No.\", " +
    "\"Registration Year\", " +
    "\"Production Year\"";
Console.WriteLine(userMessage);

InferenceParams inferenceParams = new InferenceParams()
{
    MaxTokens = 8000, // No more than 256 tokens should appear in answer. Remove it if antiprompt is enough for control.
    AntiPrompts = new List<string> { "User:" } // Stop generation once antiprompts appear.
};

ChatSession session = new(executor, chatHistory);

await foreach ( // Generate the response streamingly.
    var text
    in session.ChatAsync(
        new ChatHistory.Message(AuthorRole.User, userMessage),
        inferenceParams))
{
    Console.ForegroundColor = ConsoleColor.White;

    if (text.Trim().EndsWith("User:"))
        Console.Write(text.Trim().Substring(0, text.Trim().Length - 5));
    else
        Console.Write(text);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine();
Console.WriteLine("Chat completed.");
Console.ForegroundColor = ConsoleColor.White;

Console.ReadKey();