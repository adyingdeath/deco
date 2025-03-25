package com.adyingdeath.parser;

import com.adyingdeath.parser.DecoLexer;
import com.adyingdeath.parser.DecoParser;

import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.IOException;
import java.nio.file.Paths;

public class Main {
    public static void main(String[] args) {
        System.out.println("Deco语言解析器示例");
        
        try {
            // 从文件加载Deco代码
            String filePath = "src/main/resources/example.deco";
            System.out.println("正在解析文件: " + filePath);
            
            // 创建词法分析器
            DecoLexer lexer = new DecoLexer(CharStreams.fromPath(Paths.get(filePath)));
            // 创建词法符号流
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // 创建语法分析器
            DecoParser parser = new DecoParser(tokens);
            // 解析代码
            ParseTree tree = parser.program();
            
            // 打印语法树
            System.out.println("解析成功！语法树:");
            System.out.println(tree.toStringTree(parser));
            
        } catch (IOException e) {
            System.err.println("文件读取错误: " + e.getMessage());
            e.printStackTrace();
        } catch (Exception e) {
            System.err.println("解析错误: " + e.getMessage());
            e.printStackTrace();
        }
    }
}