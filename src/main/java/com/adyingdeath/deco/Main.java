package com.adyingdeath.deco;

import com.adyingdeath.deco.parser.DecoParser;
import com.adyingdeath.deco.parser.DecoLexer;
import com.adyingdeath.deco.sandbox.Sandbox;
import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.IOException;
import java.nio.file.Paths;

public class Main {
    public static void main(String[] args) {
        Sandbox sandbox = new Sandbox();
        System.out.println("Deco Example");
        
        try {
            // Load Deco code from file
            String filePath = "src/main/resources/example.deco";
            System.out.println("Parsing file: " + filePath);
            
            // Create lexer
            DecoLexer lexer = new DecoLexer(CharStreams.fromPath(Paths.get(filePath)));
            // Create token stream
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            // Create parser
            DecoParser parser = new DecoParser(tokens);
            parser.addParseListener(new DecoListener(sandbox));
            // Parse code
            ParseTree tree = parser.program();
            
            // Print syntax tree
            System.out.println("Done! Tree: ");
            System.out.println(tree.toStringTree(parser));
            
        } catch (IOException e) {
            System.err.println("Wrong: " + e.getMessage());
            e.printStackTrace();
        }
    }
}