using Antlr4.Runtime;
using Deco.Ast;
using Deco.Compiler;
using Deco.Compiler.Passes;

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
int test1 = 0;
void main() {
    for (int a = 3 + 4;a < 5;a = a + 1 + 2) {
        print(a + b + c);
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

        var constant_folding_ast = new ConstantFoldingPass().Visit(ast);

        var for_loop_to_while_ast = new ForLoopToWhilePass().Visit(constant_folding_ast);

        var expression_linearization_ast = new ExpressionLinearizationPass().Visit(for_loop_to_while_ast);

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
}
