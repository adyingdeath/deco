package com.adyingdeath.deco;

import com.adyingdeath.deco.parser.DecoParser;
import com.adyingdeath.deco.parser.DecoLexer;
import com.adyingdeath.deco.sandbox.Sandbox;
import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.IOException;
import java.nio.file.Paths;

/**
 * The Deco language compiler
 */
public class Compiler {
    private final boolean verbose;
    private final Sandbox sandbox;
    
    /**
     * Constructor for the Compiler
     * 
     * @param verbose Whether to output verbose information
     */
    public Compiler(boolean verbose) {
        this.verbose = verbose;
        this.sandbox = new Sandbox();
    }
    
    /**
     * Compile a source file
     * 
     * @param sourcePath The source file path
     * @param outputPath The output file path
     * @return true if compilation was successful, false otherwise
     */
    public boolean compile(String sourcePath, String outputPath) {
        try {
            if (verbose) {
                System.out.println("Parsing file: " + sourcePath);
                System.out.println("Output file: " + outputPath);
            }
            
            // Create lexer
            DecoLexer lexer = new DecoLexer(CharStreams.fromPath(Paths.get(sourcePath)));
            
            // Create token stream
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            
            // Create parser
            DecoParser parser = new DecoParser(tokens);
            parser.addParseListener(new DecoListener(sandbox));
            
            // Parse code
            ParseTree tree = parser.program();
            
            // Print syntax tree if in verbose mode
            if (verbose) {
                System.out.println("Done! Tree: ");
                System.out.println(tree.toStringTree(parser));
            }
            
            // TODO: Add code generation and output writing
            // For now, we just return true to indicate success
            return true;
            
        } catch (IOException e) {
            System.err.println("Error: " + e.getMessage());
            if (verbose) {
                e.printStackTrace();
            }
            return false;
        }
    }
}