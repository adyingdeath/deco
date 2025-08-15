

using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;
using System;
using System.IO;
using System.Linq;
using System.Text;

// --- Stage 1: Source Code Input ---
string sourceCode = @"
tick main_tick() {
    @`scoreboard players add @p ticks 1`;
    @`say running tick...`;
}

load main_load() {
    @`say pack loaded!`;
}
";

Console.WriteLine("--- Source Code ---");
Console.WriteLine(sourceCode);
Console.WriteLine("-------------------");

// --- Stage 2: Parsing the Code ---
Console.WriteLine("\n--- Parsing Stage ---");
ICharStream stream = CharStreams.fromString(sourceCode);
DecoLexer lexer = new DecoLexer(stream);
CommonTokenStream tokens = new CommonTokenStream(lexer);
DecoParser parser = new DecoParser(tokens);
var tree = parser.program();
Console.WriteLine("Parsing complete.");

// --- Stage 3: Visiting the Parse Tree to Populate Data Model ---
Console.WriteLine("\n--- Visitor Stage ---");
var dataPack = new DataPack("generated_datapack");
var visitor = new DecoCodeVisitor(dataPack);
visitor.Visit(tree);
Console.WriteLine($"Visitor finished. Found {dataPack.Functions.Count} functions and {dataPack.Tags.Count} tags.");

// --- Stage 4: Generating .mcfunction Files ---
Console.WriteLine("\n--- Function Generation Stage ---");
string rootPath = dataPack.Name;
if (Directory.Exists(rootPath))
{
    Directory.Delete(rootPath, true);
}
Directory.CreateDirectory(rootPath);

foreach (var function in dataPack.Functions)
{
    string functionDirectory = Path.Combine(rootPath, "data", function.Location.Namespace, "functions");
    Directory.CreateDirectory(functionDirectory);
    string filePath = Path.Combine(functionDirectory, $"{function.Location.Path}.mcfunction");
    
    File.WriteAllLines(filePath, function.Commands);
    Console.WriteLine($"  -> Generated function: {Path.GetFullPath(filePath)}");
}

// --- Stage 5: Generating Generic Tag Files ---
Console.WriteLine("\n--- Tag Generation Stage ---");
foreach (var tag in dataPack.Tags)
{
    // Determine the directory based on the tag type (e.g., "functions", "blocks")
    string tagTypeDirectoryName = tag.Type.ToString().ToLowerInvariant() + "s"; // E.g., Functions -> functions
    
    string tagDirectory = Path.Combine(rootPath, "data", tag.Location.Namespace, "tags", tagTypeDirectoryName);
    Directory.CreateDirectory(tagDirectory);
    string filePath = Path.Combine(tagDirectory, $"{tag.Location.Path}.json");

    // Build the JSON content
    var jsonBuilder = new StringBuilder();
    jsonBuilder.AppendLine("{");
    jsonBuilder.AppendLine("  \"values\": [");
    jsonBuilder.Append(string.Join(",\n", tag.Values.Select(v => $"    \"{v}\"")));
    jsonBuilder.AppendLine();
    jsonBuilder.AppendLine("  ]");
    jsonBuilder.AppendLine("}");

    File.WriteAllText(filePath, jsonBuilder.ToString());
    Console.WriteLine($"  -> Generated tag ({tag.Type}): {Path.GetFullPath(filePath)}");
}


Console.WriteLine("\n--- Compilation Finished ---");