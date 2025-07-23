// Program.cs
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

Console.WriteLine("Compilation finished.");