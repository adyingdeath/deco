using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;

// --- Stage 1: Source Code Input ---
string sourceCode = @"
@load
@name(""deco:test/great"")
tick main_tick() {
    @`scoreboard players add @p ticks 1`;
    @`say running tick...`;
}

@tick
load main_load() {
    @`say tick!`;
}
";

Console.WriteLine("--- Source Code ---");
Console.WriteLine(sourceCode);
Console.WriteLine("-------------------");

// --- Stage 2: Parsing the Code ---
Console.WriteLine("--- Parsing Stage ---");
ICharStream stream = CharStreams.fromString(sourceCode);
DecoLexer lexer = new DecoLexer(stream);
CommonTokenStream tokens = new CommonTokenStream(lexer);
DecoParser parser = new DecoParser(tokens);
var tree = parser.program();
Console.WriteLine("Parsing complete.");

// --- Stage 3: Visiting the Parse Tree to Populate Data Model ---
Console.WriteLine("--- Visitor Stage ---");
var dataPack = new DataPack("generated_datapack");
var visitor = new DecoCodeVisitor(dataPack);
visitor.Visit(tree);
Console.WriteLine($"Visitor finished. Found {dataPack.Functions.Count} functions and {dataPack.Tags.Count} tags.");

// --- Stage 4: Writing the Data Pack to Files ---
var writer = new PackWriter(dataPack);
writer.Write();


Console.WriteLine("\n--- Compilation Finished ---");