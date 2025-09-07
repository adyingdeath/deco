using Antlr4.Runtime;
using Deco.Ast;
using Deco.Compiler;
using Deco.Compiler.Data;
using Deco.Compiler.Passes;
using System.IO;

class Program {
    static void Main(string[] args) {
        if (args.Length == 0) {
            // No arguments, run test code
            RunTest();
        } else if (args.Length == 2) {
            string inputPath = args[0];
            string outputDirectory = args[1];

            if (File.Exists(inputPath)) {
                string dataPackName = Path.GetFileNameWithoutExtension(inputPath);
                string dataPackNamespace = dataPackName; // Use file name as namespace by default
                CompileFile(inputPath, outputDirectory, dataPackName, dataPackNamespace);
            } else if (Directory.Exists(inputPath)) {
                // [TODO] Handle directory input
                Console.WriteLine("Directory input is not yet supported.");
            } else {
                Console.WriteLine($"Error: Input path '{inputPath}' not found.");
            }
        } else {
            Console.WriteLine("Usage:");
            Console.WriteLine("  deco <input_file> <output_directory>");
            Console.WriteLine("  deco (runs tests)");
        }
    }

    static void RunTest() {
        string sourceCode = File.ReadAllText($"D:\\programming\\project\\deco\\test\\ast_builder_test\\nested_if.deco");

        var preprocessor = new DecoPreprocessor();
        string processedCode = preprocessor.Preprocess(@"
void main() {
    if (true == true) {
    }
}
        ");

        ICharStream stream = CharStreams.fromString(processedCode);
        DecoLexer lexer = new DecoLexer(stream);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        DecoParser parser = new DecoParser(tokens);
        var tree = parser.program();

        var astBuilder = new AstBuilder();
        var ast = astBuilder.Visit(tree);

        var new_ast = new ConstantFoldingPass().Visit(ast);

        return;

        Console.WriteLine("--- Running in Test Mode ---");
        string[] testList = [
            "argument_passing",
            "expression_evaluation",
            "boolean_operation",
            "if_statement",
            "minecraft_condition_expression",
            "while_loop_test",
            "unary_minus_test",
            "return_statement",

            "deco_core_lib\\function_test",
        ];
        string testFileName = testList[1];
        string inputFile = $"D:\\programming\\project\\deco\\test\\{testFileName}.deco";
        string outputDirectory = "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks";

        CompileFile(inputFile, outputDirectory, "test", "test");
    }

    static void CompileFile(string inputFile, string outputDirectory, string dataPackName, string dataPackNamespace) {
        Console.WriteLine($"--- Compiling {inputFile} ---");
        Console.WriteLine($"Output directory: {outputDirectory}");
        Console.WriteLine($"Datapack name/namespace: {dataPackName}");

        // --- Stage 1: Source Code Input ---
        Console.WriteLine($"--- Reading Source Code from {inputFile} ---");
        string sourceCode = File.ReadAllText(inputFile);
        Console.WriteLine("-------------------");

        // --- Stage 1.5: Preprocessing ---
        Console.WriteLine("--- Preprocessing Stage ---");
        var preprocessor = new DecoPreprocessor();
        string processedCode = preprocessor.Preprocess(sourceCode);
        Console.WriteLine("Preprocessing complete.");

        // --- Stage 2: Parsing the Code ---
        Console.WriteLine("--- Parsing Stage ---");
        ICharStream stream = CharStreams.fromString(processedCode);
        DecoLexer lexer = new DecoLexer(stream);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        DecoParser parser = new DecoParser(tokens);
        var tree = parser.program();
        Console.WriteLine("Parsing complete.");

        // --- Stage 3: Visiting the Parse Tree to Populate Data Model ---
        Console.WriteLine("--- Visitor Stage ---");
        var dataPack = new DataPack("ukx34rhy", dataPackName, dataPackNamespace);

        // Pass 1: Discover all function signatures
        var discoveryVisitor = new SymbolCollector(dataPack);
        discoveryVisitor.Visit(tree);
        Console.WriteLine($"Discovery pass finished. Found {dataPack.Functions.DecoFunctions.Count} function signatures.");

        // Pass 2: Visit the parse tree to generate code with library system
        Console.WriteLine("--- Initializing Library System ---");
        var codeVisitor = new DecoCompiler(dataPack);
        codeVisitor.InitializeLibrarySystem(); // Initialize library system in the compiler
        Console.WriteLine($"Library system loaded. Available types: {codeVisitor.Registry.GetAllTypes().Count()}, functions: {codeVisitor.Registry.GetAllFunctions().Count()}");

        codeVisitor.GenerateCode();
        Console.WriteLine($"Visitor finished. Found {dataPack.Functions.McFunctions.Count} functions and {dataPack.Tags.Count} tags.");

        // --- Stage 4: Writing the Data Pack to Files ---
        Console.WriteLine("--- Writing Output ---");
        var writer = new PackWriter(dataPack, outputDirectory);
        writer.Write();

        Console.WriteLine("--- Compilation Finished ---");
    }
}
