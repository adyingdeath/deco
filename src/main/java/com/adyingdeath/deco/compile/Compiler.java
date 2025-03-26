package com.adyingdeath.deco.compile;

import com.adyingdeath.deco.parser.DecoParser;
import com.adyingdeath.deco.parser.DecoLexer;
import com.adyingdeath.deco.sandbox.Sandbox;
import org.antlr.v4.runtime.CharStreams;
import org.antlr.v4.runtime.CommonTokenStream;
import org.antlr.v4.runtime.tree.ParseTree;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.io.File;

/**
 * The Deco language compiler
 */
public class Compiler {
    private final Sandbox sandbox;
    
    /**
     * Constructor for the Compiler
     * 
     * @param sandbox Sandbox used to store compilation information
     */
    public Compiler(Sandbox sandbox) {
        this.sandbox = sandbox;
    }
    
    /**
     * Compile a source file
     * 
     * @param src The source deco file
     * @return true if compilation was successful, false otherwise
     */
    public boolean compile(DecoFile src) {
        // Create lexer
        DecoLexer lexer = new DecoLexer(CharStreams.fromString(src.getContent()));

        // Create token stream
        CommonTokenStream tokens = new CommonTokenStream(lexer);

        // Create parser
        DecoParser parser = new DecoParser(tokens);
        parser.addParseListener(new DecoListener(sandbox));

        // Parse code
        parser.program();

        // For now, we just return true to indicate success
        return true;
    }
    
    /**
     * Compile a source file from file path
     * 
     * @param srcFilePath The path to the source deco file
     * @param outputFilePath The path to the output file
     * @return true if compilation was successful, false otherwise
     */
    public boolean compile(String srcFilePath, String outputFilePath) {
        try {
            // Read file content
            String content = Files.readString(Path.of(srcFilePath));
            
            // Extract file information
            Path path = Paths.get(srcFilePath);
            String fileName = path.getFileName().toString();
            
            // Extract namespace and path from the file path
            // This is a simplified approach and might need to be adjusted based on your datapack structure
            String[] parts = srcFilePath.split("data" + File.separator);
            if (parts.length < 2) {
                System.err.println("Invalid file path structure: " + srcFilePath);
                return false;
            }
            
            String[] pathParts = parts[1].split(File.separator);
            if (pathParts.length < 3) {
                System.err.println("Invalid path structure within datapack: " + parts[1]);
                return false;
            }
            
            String namespace = pathParts[0];
            
            // Build the function path (everything between namespace and filename)
            StringBuilder functionPath = new StringBuilder();
            for (int i = 2; i < pathParts.length - 1; i++) {
                if (functionPath.length() > 0) {
                    functionPath.append("/");
                }
                functionPath.append(pathParts[i]);
            }
            
            // Extract base name (without extension)
            String baseName = fileName;
            int lastDotIndex = fileName.lastIndexOf('.');
            if (lastDotIndex > 0) {
                baseName = fileName.substring(0, lastDotIndex);
            }
            
            // Create DecoFile object
            DecoFile decoFile = new DecoFile(namespace, functionPath.toString(), baseName, content);
            
            // Compile using the DecoFile object
            return compile(decoFile);
            
        } catch (IOException e) {
            System.err.println("Error reading source file: " + e.getMessage());
            return false;
        }
    }
}