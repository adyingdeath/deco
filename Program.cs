using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;

// --- Stage 1: Source Code Input ---
string sourceCode = @"
@load
int main_tick() {
    main_load(1, ""aaa"", 1, 2);
}

void main_load(int a, string te, float cc, int ff) {
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
var dataPack = new DataPack("ukx34rhy", "generated_datapack", "deco");
// Pass 1: Discover all function signatures
var discoveryVisitor = new SymbolCollector(dataPack);
discoveryVisitor.Visit(tree);
Console.WriteLine($"Discovery pass finished. Found {dataPack.FunctionTable.Count} function signatures.");
Console.WriteLine(dataPack);

// Pass 2: Visit the parse tree to generate code
var codeVisitor = new DecoCompiler(dataPack);
codeVisitor.GenerateCode();
Console.WriteLine($"Visitor finished. Found {dataPack.Functions.Count} functions and {dataPack.Tags.Count} tags.");

// --- Stage 4: Writing the Data Pack to Files ---
var writer = new PackWriter(dataPack);
writer.Write();


Console.WriteLine("\n--- Compilation Finished ---");