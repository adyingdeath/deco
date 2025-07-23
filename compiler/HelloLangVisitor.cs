// MyHelloVisitor.cs

// 继承自 ANTLR 生成的 HelloBaseVisitor<T>
// T 是你希望 visit 方法返回的类型，这里我们用 object 就好
public class HelloLangVisitor : HelloBaseVisitor<object>
{
    // 重写 VisitGreet 方法，因为我们的语法规则叫 'greet'
    public override object VisitGreet(HelloParser.GreetContext context)
    {
        // context.ID() 可以让我们访问到规则中匹配到的 ID 部分
        // .GetText() 可以获取这个 ID 的文本内容
        string name = context.ID().GetText();

        // 执行我们的核心逻辑！
        Console.WriteLine($"Compiler says: Hello to you, {name}!");

        return null; // 返回值在这里不重要
    }
}