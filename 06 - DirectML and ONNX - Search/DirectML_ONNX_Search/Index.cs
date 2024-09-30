using DirectML_ONNX_Search;
using DirectML_ONNX_Search.VectorDB;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace DirectML_ONNX_Search
{
    internal class Index
    {

        RAGService _ragService = new RAGService();
        SLMRunner _smlRunner = new SLMRunner();

        public async Task InitializeSMLRunnerAsync()
        {
            await _smlRunner.InitializeAsync();
        }

        public async Task IndexFolder(string path)
        {
            var files = System.IO.Directory.GetFiles(path, "*.md", System.IO.SearchOption.AllDirectories);
            var myRegex = new Regex(@"[\u0000-\u001F\u007F-\uFFFF]");

            var contents = new List<TextChunk>();
            
            // 1) Read all documentation files
            foreach (var filePath in files)
            {
                var builder = MDHelper.RemoveTags(File.ReadAllText(filePath));

                var range = builder
                    .Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => myRegex.Replace(x, ""))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new TextChunk
                    {
                        Text = x,
                        FilePath = filePath
                    });
                contents.AddRange(range);
            }


            // 2) Split the text into chunks to make sure they are
            // smaller than what the Embeddings model supports
            var maxLength = 1024 / 2;
            for (int i = 0; i<contents.Count; i++)
            {
                var content = contents[i];
                int index = 0;
                var contentChunks = new List<TextChunk>();
                while (index<content.Text!.Length)
                {
                    if (index + maxLength >= content.Text.Length)
                    {
                        contentChunks.Add(new TextChunk(content)
                        {
                            Text = Regex.Replace(content.Text[index..].Trim(), @"(\.){2,}", ".")
                        });
                        break;
                    }

                    int lastIndexOfBreak = content.Text.LastIndexOf(' ', index + maxLength, maxLength);
                    if (lastIndexOfBreak <= index)
                    {
                        lastIndexOfBreak = index + maxLength;
                    }

                    contentChunks.Add(new TextChunk(content)
                    {
                        Text = Regex.Replace(content.Text[index..lastIndexOfBreak].Trim(), @"(\.){2,}", ".")
                    }); ;

                    index = lastIndexOfBreak + 1;
                }

                contents.RemoveAt(i);
                contents.InsertRange(i, contentChunks);
                i += contentChunks.Count - 1;
            }

            //extend chunks context
            var maxLongTextLength = 1300;
            for (int i=0; i< contents.Count; i++)
            {
                contents[i].LongText = contents[i].Text;
                if ((i>0) && (contents[i].FilePath == contents[i - 1].FilePath))
                {
                    contents[i].LongText = contents[i - 1].LongText + " " + contents[i].Text;
                    if (contents[i].LongText!.Length > maxLongTextLength)
                        contents[i].LongText = contents[i].LongText!.Substring(contents[i].LongText!.Length - maxLongTextLength);
                }
                if ((i < (contents.Count - 1)) && (contents[i].FilePath == contents[i + 1].FilePath))
                {
                    contents[i].LongText = contents[i].LongText + " " + contents[i + 1].Text;
                    if (contents[i].LongText!.Length > maxLongTextLength)
                        contents[i].LongText = contents[i].LongText!.Substring(0, maxLongTextLength);
                }
            }

            // 3) Index the chunks
            await _ragService.InitializeAsync(contents);
        }

        public async Task SearchNoChatAI(string question)
        {
            // 4) Search the chunks using the user's prompt, with the same model used for indexing
            var contents = (await _ragService.Search(question, 2)).OrderBy(c => c.FilePath);

            foreach (var content in contents)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"File {Path.GetFileName(content.FilePath)}:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(content.Text);
                Console.WriteLine();
            }
        }

        public async Task Search(string question)
        {
            var prompt = """
        <|system|>
        You are a helpful assistant, and you should answer user question, in a direct and simple way, using only this content:
        """;

            // 4) Search the chunks using the user's prompt, with the same model used for indexing
            var contents = (await _ragService.Search(question, 2)).OrderBy(c => c.FilePath);

            var filesChunks = contents.GroupBy(c => c.FilePath)
                .Select(g => $"{Environment.NewLine}File {g.Key}: {string.Join(Environment.NewLine, g.Select(c => c.LongText))}").ToList();

            prompt += string.Join(Environment.NewLine, filesChunks);

            prompt += $"""
        <|end|>
        <|user|>
        {question}<|end|>
        <|assistant|>
        """;

            Console.ForegroundColor = ConsoleColor.Yellow;
            var filesChunksList = contents.GroupBy(c => c.FilePath)
                .Select(g => $"{Environment.NewLine}File {g.Key}\n").ToList();
            Console.WriteLine("Information found in these files:");
            foreach (var filePath in filesChunksList)
                Console.WriteLine("  " + System.IO.Path.GetFileName(filePath));
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.White;

            // 5) Use Phi3 to generate the answer
            await foreach (var partialResult in _smlRunner.InferStreamingAsync(prompt))
            {
                Console.Write(partialResult);
            }

            //write source data
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("Information based on:");
            Console.WriteLine(string.Join(Environment.NewLine, filesChunks));
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
        }


    }
}
