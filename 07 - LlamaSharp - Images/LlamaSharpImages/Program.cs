﻿using System.Drawing;
using System.Text.RegularExpressions;
using LLama;
using LLama.Common;
using LLama.Native;

string multiModalProj = @"D:\Conferences\2024-09 - Local AI\00 - Models\gguff-models\llava-llama-3-8b-v1_1\\llava-llama-3-8b-v1_1-mmproj-f16.gguf";
string modelPath = @"D:\Conferences\2024-09 - Local AI\00 - Models\gguff-models\llava-llama-3-8b-v1_1\\llava-llama-3-8b-v1_1-int4.gguf";

//string modelImage = @"D:\Conferences\2024-09 - Local AI\06 - LlamaSharp - Images\ImageExamples\Eiffel.jpg";
string modelImage = @"D:\Conferences\2024-09 - Local AI\06 - LlamaSharp - Images\ImageExamples\money-coins.jpg";

const int maxTokens = 1024;

var prompt = $"USER:\nCategorize the image: {{{modelImage}}}.\nASSISTANT:\n";

var parameters = new ModelParams(modelPath);

using var model = LLamaWeights.LoadFromFile(parameters);
using var context = model.CreateContext(parameters);

// Llava Init
using var clipModel = LLavaWeights.LoadFromFile(multiModalProj);

var ex = new InteractiveExecutor(context, clipModel);

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {0} and the context size is {1}.", maxTokens, parameters.ContextSize);
Console.WriteLine("To send an image, enter its filename in curly braces, like this {c:/image.jpg}.");

var inferenceParams = new InferenceParams() { Temperature = 0.1f, AntiPrompts = new List<string> { "\nUSER:" }, MaxTokens = maxTokens };

do
{

    // Evaluate if we have images
    //
    var imageMatches = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
    var imageCount = imageMatches.Count();
    var hasImages = imageCount > 0;

    if (hasImages)
    {
        var imagePathsWithCurlyBraces = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
        var imagePaths = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Groups[1].Value).ToList();

        List<byte[]> imageBytes;
        try
        {
            imageBytes = imagePaths.Select(File.ReadAllBytes).ToList();
        }
        catch (IOException exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(
                $"Could not load your {(imageCount == 1 ? "image" : "images")}:");
            Console.Write($"{exception.Message}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please try again.");
            break;
        }

        // Each prompt with images we clear cache
        // When the prompt contains images we clear KV_CACHE to restart conversation
        // See:
        // https://github.com/ggerganov/llama.cpp/discussions/3620
        ex.Context.NativeHandle.KvCacheRemove(LLamaSeqId.Zero, -1, -1);

        int index = 0;
        foreach (var path in imagePathsWithCurlyBraces)
        {
            // First image replace to tag <image, the rest of the images delete the tag
            prompt = prompt.Replace(path, index++ == 0 ? "<image>" : "");
        }


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Here are the images, that are sent to the chat model in addition to your message.");
        Console.WriteLine();

        //foreach (var consoleImage in imageBytes?.Select(bytes => new CanvasImage(bytes)))
        //{
        //    consoleImage.MaxWidth = 50;
        //    Console.Write(consoleImage);
        //}

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The images were scaled down for the console only, the model gets full versions.");
        Console.WriteLine($"Write /exit or press Ctrl+c to return to main menu.");
        Console.WriteLine();


        // Initialize Images in executor
        //
        foreach (var image in imagePaths)
        {
            ex.Images.Add(await File.ReadAllBytesAsync(image));
        }
    }

    Console.ForegroundColor = ConsoleColor.White;
    await foreach (var text in ex.InferAsync(prompt, inferenceParams))
    {
        Console.Write(text);
    }
    Console.Write(" ");
    Console.ForegroundColor = ConsoleColor.Green;
    prompt = Console.ReadLine();
    Console.WriteLine();

    // let the user finish with exit
    //
    if (prompt != null && prompt.Equals("/exit", StringComparison.OrdinalIgnoreCase))
        break;

}
while (true);