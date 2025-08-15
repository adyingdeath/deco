/* // Program.cs
using Antlr4.Runtime;

// 我们的源代码
string sourceCode = "hello CSharp\nhello world";

// 1. 创建输入流
ICharStream stream = CharStreams.fromString(sourceCode);

// 2. 创建词法分析器
HelloLexer lexer = new HelloLexer(stream);

// 3. 创建 Token 流
CommonTokenStream tokens = new CommonTokenStream(lexer);

// 4. 创建语法分析器
HelloParser parser = new HelloParser(tokens);

// 5. 开始解析，获取 AST (抽象语法树) 的根节点
// 'greet()' 方法对应我们 .g4 文件里的 'greet' 规则
var tree = parser.greet();

// 6. 创建我们的 Visitor 实例
var visitor = new HelloLangVisitor();

// 7. 让 Visitor 访问 AST，这会触发我们重写的 VisitGreet 方法
visitor.Visit(tree);

Console.WriteLine("Compilation finished."); */


        var wrapper = new MinecraftCommandWrapper();

        // --- Test Case 1: Your basic example ---
        string code1 = @"
func test() {
execute at @s run tellraw {""text"": ""gogogo;123""};
return;
}";
        
        // --- Test Case 2: A more complex example with NBT arrays and multi-line commands ---
        string code2 = @"
func complex_stuff() {
    // This is a comment.
    give @p diamond_sword{display:{Name:'{""text"":""Semicolon;Sword""}'},Enchantments:[{id:""minecraft:sharpness"",lvl:5s}]} 1;
    
    data merge block ~ ~1 ~ 
        {Items:[{Slot:0b,id:""minecraft:stone"",Count:1b},
                {Slot:1b,id:""minecraft:dirt"", Count:1b, UUID: [I;1,2,3], tag:{note:""NBT with [I;1,2,3] inside""}}]};

    scoreboard players set Test Score 1;
}
";

        // --- Test Case 3: A command keyword immediately followed by a semicolon ---
        string code3 = "return;";

        Console.WriteLine("--- Original Code 1 ---");
        Console.WriteLine(code1);
        Console.WriteLine("\n--- Transformed Code 1 ---");
        string transformedCode1 = wrapper.Transform(code1);
        Console.WriteLine(transformedCode1);
        Console.WriteLine("======================================");

        Console.WriteLine("\n--- Original Code 2 ---");
        Console.WriteLine(code2);
        Console.WriteLine("\n--- Transformed Code 2 ---");
        string transformedCode2 = wrapper.Transform(code2);
        Console.WriteLine(transformedCode2);
        Console.WriteLine("======================================");

        Console.WriteLine("\n--- Original Code 3 ---");
        Console.WriteLine(code3);
        Console.WriteLine("\n--- Transformed Code 3 ---");
        string transformedCode3 = wrapper.Transform(code3);
        Console.WriteLine(transformedCode3);
        Console.WriteLine("======================================");
