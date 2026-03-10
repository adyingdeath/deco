using Antlr4.Runtime;
using Deco.Compiler.Ast;
using Deco.Compiler;
using Deco.Compiler.IR;
using Deco.Compiler.Pack;
using Deco.Compiler.Ast.Passes;
using Deco.Compiler.Lib; // Import PluginLoader
using Deco.Compiler.Types;

class Program {
    static void Main(string[] args) {
        if (args.Length == 0) {
            RunTest();
        } else if (args.Length == 2) {
            // [TODO] Implement file compilation logic
            Console.WriteLine("CLI args not fully supported yet. Running test.");
            RunTest();
        }
    }

    static void RunTest() {
        var preprocessor = new DecoPreprocessor();
        string processedCode = preprocessor.Preprocess(@"
int counter = 0;
void main() {
    print(counter);
    for(int i = 0; i < 3; i = i + 1) {
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

        var datapack = new Datapack("testpack", "deco");
        var context = new CompilationContext(datapack);

        // Build symbol table
        var symbolTable = new Scope(context, "global");

        // --- Passes ---
        GlobalSymbolTableBuilder.Action(context, symbolTable, ast);
        
        // --- LOAD LIBRARIES ---
        // Load built-in standard library
        PluginLoader.LoadStandardLibrary(context, symbolTable);
        // Example: PluginLoader.LoadPlugin("path/to/MyLib.dll", context, symbolTable);

        ScopedSymbolTableBuilder.Action(context, symbolTable, ast);
        IdentifierUsageChecker.Action(context, symbolTable, ast);
        ast = (ProgramNode)TypeResolver.Action(context, symbolTable, ast);

        ast = new ForLoopToWhilePass().Visit(ast);
        ast = new ConstantFoldingPass().Visit(ast);

        // --- IR Generation ---
        var irBuilder = new IRBuilder(context);
        var irProgram = irBuilder.Build((ProgramNode)ast);

        if (context.ErrorReporter.HasErrors) {
            context.ErrorReporter.PrintAll();
            return;
        }

        // --- Output IR Debug ---
        var irs_str = GenerateIrString(irProgram);
        File.WriteAllText("./irs.txt", irs_str);

        // --- Datapack Generation ---
        new DatapackBuilder(context).VisitProgram(irProgram);

        // Export
        // Change path as needed
        //string exportPath = Path.Combine(Directory.GetCurrentDirectory(), "output", "datapacks", "deco_test");
        string exportPath = "D:\\Program Files\\minecraft\\hmcl\\.minecraft\\versions\\1.21\\saves\\deco test\\datapacks\\deco";
        DatapackExporter.Export(datapack, exportPath);
        Console.WriteLine($"Exported to: {exportPath}");
    }

    static string GenerateIrString(IrProgram program) {
        List<string> lines = [$"DataPack: {program.DataPackId}"];
        foreach (var func in program.Functions) {
            lines.Add(func.ToString());
        }
        return string.Join("\n", lines);
    }
}