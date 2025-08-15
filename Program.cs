

using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;
using System;
using System.IO;

// --- Stage 1: Source Code Input ---
string sourceCode = @"
void tick() {
    @`say hello from tick`;
    @`scoreboard players add @a ticks 1`;
}

void load() {
    @`say loading...`;
}";

Console.WriteLine("--- Source Code ---");
Console.WriteLine(sourceCode);
Console.WriteLine("-------------------");

// --- Stage 2: Parsing the Code ---
Console.WriteLine("\n--- Parsing Stage ---");
ICharStream stream = CharStreams.fromString(sourceCode);
DecoLexer lexer = new DecoLexer(stream);
CommonTokenStream tokens = new CommonTokenStream(lexer);
DecoParser parser = new DecoParser(tokens);
var tree = parser.program(); // Get the parse tree
Console.WriteLine("Parsing complete.");

// --- Stage 3: Visiting the Parse Tree to Populate Data Model ---
Console.WriteLine("\n--- Visitor Stage ---");
var dataPack = new DataPack("generated_datapack");
var visitor = new DecoCodeVisitor(dataPack);
visitor.Visit(tree); // This fills the dataPack object with functions and commands
Console.WriteLine($"Visitor finished. Found {dataPack.Functions.Count} functions.");

// --- Stage 4: Generating Files from the Data Model ---
Console.WriteLine("\n--- File Generation Stage ---");
string rootPath = dataPack.Name;
if (Directory.Exists(rootPath))
{
    Directory.Delete(rootPath, true);
}
Directory.CreateDirectory(rootPath);

foreach (var function in dataPack.Functions)
{
    string functionDirectory = Path.Combine(rootPath, "data", function.Namespace, "functions");
    Directory.CreateDirectory(functionDirectory);
    string filePath = Path.Combine(functionDirectory, $"{function.Name}.mcfunction");
    
    File.WriteAllLines(filePath, function.Commands);
    Console.WriteLine($"  -> Generated file: {Path.GetFullPath(filePath)}");
}

Console.WriteLine("\n--- Compilation Finished ---");
