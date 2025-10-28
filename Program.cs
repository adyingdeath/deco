using Antlr4.Runtime;
using Deco.Compiler.Ast;
using Deco.Compiler;
using Deco.Compiler.IR;
using Deco.Compiler.IR.Passes;
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
        string sourceCode = File.ReadAllText($"D:\\programming\\project\\deco\\test\\ast_builder_test\\nested_if.deco");

        var preprocessor = new DecoPreprocessor();
        string processedCode = preprocessor.Preprocess(@"
int counter = 0;
void main() {
    counter = 6 + 1;
    int result = 567;
    result = chain(counter);
    print(result);
}
int chain(int a) {
    print(a);
    if (a == 1) {
        return 1;
    }
    return a * chain(a - 1);
}
        ");

        ICharStream stream = CharStreams.fromString(processedCode);
        DecoLexer lexer = new(stream);
        CommonTokenStream tokens = new(lexer);
        DecoParser parser = new(tokens);
        var tree = parser.program();

        var astBuilder = new AstBuilder();
        var ast = astBuilder.Visit(tree);

        new FindFatherPass().Visit(ast);

        var datapack = new Datapack("6u753i8", "deco");
        var context = new CompilationContext(datapack);

        // Build symbol table
        var symbolTable = new Scope(context, "global");

        // ~~~~~~~~~~~ Collect Symbols ~~~~~~~~~~~ //
        // Collect symbols and build nested symbol table.
        // This includes two steps currently:
        // 1. Build global symbol table;
        // 2. Build scoped symbol table;
        GlobalSymbolTableBuilder.Action(context, symbolTable, ast);
        ScopedSymbolTableBuilder.Action(context, symbolTable, ast);
        // Collect symbols for library functions.
        LibraryFunctionSymbolCollector.Build(context, symbolTable, [new PrintFunction()]);
        // Check identifier usage
        IdentifierUsageChecker.Action(context, symbolTable, ast);


        // ~~~~~~~~~~~~~ Handle Type ~~~~~~~~~~~~~ //
        // Type check and resolve types
        ast = (ProgramNode)TypeResolver.Action(symbolTable, ast);


        // ~~~~~~~~~~~ AST Optimization ~~~~~~~~~~ //
        ast = new ConstantFoldingPass().Visit(ast);
        ast = new ForLoopToWhilePass().Visit(ast);

        // var expression_linearization_ast = new ExpressionLinearizationPass().Visit(for_loop_to_while_ast);


        var irs = ast.Accept(new IRBuilder(context));

        var program = NestInstructionPass.Visit(irs);

        program = LinkMergePass.Visit(program);

        // Generate string from nested structure
        var irs_str = GenerateNestedString(program);

        File.WriteAllText("./irs.txt", irs_str);

        new DatapackBuilder(context).VisitProgram(program);

        DatapackExporter.Export(datapack, "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks\\deco");

        return;

        /* Console.WriteLine("--- Running in Test Mode ---");
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
        string outputDirectory = "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks"; */
    }

    static string GenerateNestedString(ProgramInstruction program) {
        List<string> lines = [program.ToString()];
        foreach (var label in program.Labels) {
            lines.Add(label.ToString());
            foreach (var instr in label.Instructions) {
                lines.Add("  " + instr.ToString());
            }
        }
        return string.Join("\n", lines);
    }
}
