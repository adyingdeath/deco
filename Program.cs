using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // TODO: Add argument parsing for input and output paths
        string inputFile = "D:\\programming\\project\\deco_csharp\\input.deco";
        string outputDirectory = "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks\\bridge";

        // --- Stage 1: Source Code Input ---
        Console.WriteLine($"--- Reading Source Code from {inputFile} ---");
        string sourceCode = File.ReadAllText(inputFile);
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
        var dataPack = new DataPack("ukx34rhy", outputDirectory, "deco");
        
        // Pass 1: Discover all function signatures
        var discoveryVisitor = new SymbolCollector(dataPack);
        discoveryVisitor.Visit(tree);
        Console.WriteLine($"Discovery pass finished. Found {dataPack.Functions.DecoFunctions.Count} function signatures.");
        
        // Pass 2: Visit the parse tree to generate code
        var codeVisitor = new DecoCompiler(dataPack);
        codeVisitor.GenerateCode();
        Console.WriteLine($"Visitor finished. Found {dataPack.Functions.McFunctions.Count} functions and {dataPack.Tags.Count} tags.");

        // --- Stage 4: Writing the Data Pack to Files ---
        Console.WriteLine("--- Writing Output ---");
        var writer = new PackWriter(dataPack);
        writer.Write();
        
        Console.WriteLine("--- Compilation Finished ---");
    }
}
