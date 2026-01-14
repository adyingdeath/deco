using Antlr4.Runtime;
using Deco.Compiler.Ast;
using Deco.Compiler;
using Deco.Compiler.IR;
using Deco.Compiler.Pack;
using Deco.Compiler.Ast.Passes;
using Deco.Compiler.Lib.Core;
using Deco.Compiler.Types;

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
                //CompileFile(inputPath, outputDirectory, dataPackName, dataPackNamespace);
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
        var preprocessor = new DecoPreprocessor();
        string processedCode = preprocessor.Preprocess(@"
int counter = 0;
void main() {
    for(int i = 0;i < 7;i = i + 1) {
        counter = counter + i;
        print(i);
    }
    print(counter);
}
        ");

        ICharStream stream = CharStreams.fromString(processedCode);
        DecoLexer lexer = new(stream);
        CommonTokenStream tokens = new(lexer);
        DecoParser parser = new(tokens);
        var tree = parser.program();

        var astBuilder = new AstBuilder();
        var ast = astBuilder.Visit(tree);
        ast.SetChildrenParent();

        var datapack = new Datapack("6u753i8", "deco");
        var context = new CompilationContext(datapack);

        // Build symbol table
        var symbolTable = new Scope(context, "global");

        // --- Passes ---
        GlobalSymbolTableBuilder.Action(context, symbolTable, ast);
        ScopedSymbolTableBuilder.Action(context, symbolTable, ast);
        LibraryFunctionSymbolCollector.Build(context, symbolTable, [new PrintFunction()]);
        IdentifierUsageChecker.Action(context, symbolTable, ast);
        ast = (ProgramNode)TypeResolver.Action(context, symbolTable, ast);

        ast = new ForLoopToWhilePass().Visit(ast);
        ast = new ConstantFoldingPass().Visit(ast);

        // --- IR Generation ---
        // IRBuilder now returns an IrProgram, not just instructions
        var irBuilder = new IRBuilder(context);
        var irProgram = irBuilder.Build((ProgramNode)ast);

        // NestInstructionPass and LinkMergePass are likely obsolete with this new structure
        // as we don't have nested labels or link instructions anymore.
        // If optimizations are needed, they would operate on IrFunction.Instructions.

        if (context.ErrorReporter.HasErrors) {
            context.ErrorReporter.PrintAll();
            return;
        }

        // --- Output ---
        var irs_str = GenerateIrString(irProgram);
        File.WriteAllText("./irs.txt", irs_str);

        // --- Datapack Generation ---
        // DatapackBuilder now visits IrProgram
        new DatapackBuilder(context).VisitProgram(irProgram);

        DatapackExporter.Export(datapack, "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks\\deco");
    }

    static string GenerateIrString(IrProgram program) {
        List<string> lines = [$"DataPack: {program.DataPackId}"];
        foreach (var func in program.Functions) {
            lines.Add(func.ToString());
        }
        return string.Join("\n", lines);
    }
}