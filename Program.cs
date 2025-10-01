using Antlr4.Runtime;
using Deco.Compiler.Ast;
using Deco.Compiler;
using Deco.Compiler.IR;
using Deco.Compiler.IR.Passes;
using Deco.Compiler.Ast.Passes.Lowering;
using Deco.Compiler.Pack;

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
int test1 = 5;
int main() {
    int result = test(6, 2);
    return result + 1;
}
int test(int a, int b) {
    if (test1 < a) {
        test1 = a;
        return b;
    } else if (test1 < a + 1) {
        int c = test1 + 1;
    }
    test1 = test1 + 2;
    return test1;
}
        ");

        ICharStream stream = CharStreams.fromString(processedCode);
        DecoLexer lexer = new DecoLexer(stream);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        DecoParser parser = new DecoParser(tokens);
        var tree = parser.program();

        var astBuilder = new AstBuilder();
        var ast = astBuilder.Visit(tree);

        new Deco.Compiler.Ast.Passes.FindFatherPass().Visit(ast);

        // Build symbol table
        var globalSymbolTable = new Deco.Types.Scope("global");

        // Collect symbols and build nested symbol table.
        // This includes two steps currently:
        // 1. Build global symbol table;
        // 2. Build scoped symbol table;
        new Deco.Compiler.Ast.Passes.Collect_Symbol.Group(globalSymbolTable).Visit(ast);

        // Type check and resolve types
        var typedAst = (ProgramNode)new Deco.Compiler.Ast.Passes.Types.Group(globalSymbolTable).Visit(ast);

        var constant_folding_ast = new ConstantFoldingPass().Visit(typedAst);

        var for_loop_to_while_ast = new ForLoopToWhilePass().Visit(constant_folding_ast);

        // var expression_linearization_ast = new ExpressionLinearizationPass().Visit(for_loop_to_while_ast);

        var irs = for_loop_to_while_ast.Accept(new IRBuilder());

        var program = NestInstructionPass.Visit(irs);

        program = LinkMergePass.Visit(program);

        // Generate string from nested structure
        var irs_str = GenerateNestedString(program);

        File.WriteAllText("./irs.txt", irs_str);

        var datapack = new Datapack("6u753i8", "deco");
        new DatapackBuilder(datapack).VisitProgram(program);

        DatapackExporter.Export(datapack, "./datapack");

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
