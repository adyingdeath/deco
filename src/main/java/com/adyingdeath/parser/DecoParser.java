package com.adyingdeath.parser;

import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.IOException;

public class DecoParser {

    public static void main(String[] args) {
        // 示例的Deco代码
        String decoCode = """
                @deprecated
                @async
                func myFunction {
                }
                
                func anotherFunction {
                }
                """;

        try {
            // 创建词法分析器
            DecoLexer lexer = new DecoLexer(CharStreams.fromString(decoCode));
            // 创建词法符号流
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // 创建语法分析器
            Deco parser = new Deco(tokens);
            // 解析代码
            ParseTree tree = parser.program();
            
            // 打印语法树 (简单的字符串表示)
            System.out.println("解析成功！语法树:");
            System.out.println(tree.toStringTree(parser));
            
            // 这里可以使用Visitor模式处理语法树
            // DecoBaseVisitor<Void> visitor = new MyDecoVisitor();
            // visitor.visit(tree);
            
        } catch (Exception e) {
            System.err.println("解析错误: " + e.getMessage());
            e.printStackTrace();
        }
    }
} 