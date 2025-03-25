package com.adyingdeath.deco;

import java.util.ArrayList;
import java.util.List;
import java.io.File;

/**
 * Entry point for the Deco compiler
 */
public class Entry {
    static final String VERSION = "0.1";
    static final String HELP = """
    Usage: java -jar deco.jar [options] <input_directory>
    Options:
      --output, -o <path>  Specify output datapack directory (default: ./data)
      --version            Display version information
      --help, -h           Display this help message
    """;

    // Options that can be set via command line
    static String outputPath = "./data";
    static String srcPath = null;
    
    /**
     * The main entry point for the Deco compiler.
     * @param args Command line arguments
     */
    public static void main(String[] args) {
        // Parse command line arguments
        Entry.parseArguments(args);
        
        // Check if input directory is specified
        if (srcPath == null) {
            System.err.println("Error: No input directory specified");
            System.out.println(Entry.HELP);
            return;
        }
        
        // Validate input directory
        File inputDir = new File(srcPath);
        if (!inputDir.exists()) {
            System.err.println("Error: Input directory does not exist: " + srcPath);
            return;
        }
        
        if (!inputDir.isDirectory()) {
            System.err.println("Error: Specified input path is not a directory: " + srcPath);
            return;
        }
        
        // Create datapack compiler and process the input directory
        DatapackCompiler datapackCompiler = new DatapackCompiler(srcPath, outputPath);
        boolean success = datapackCompiler.compile();
        
        if (success) {
            System.out.println("Datapack compilation completed successfully");
            System.out.println("Output datapack: " + outputPath);
        } else {
            System.err.println("Datapack compilation failed");
        }
    }
    
    /**
     * Parse command line arguments
     * @param args Command line arguments
     */
    private static void parseArguments(String[] args) {
        for (int i = 0; i < args.length; i++) {
            switch (args[i]) {
                case "--output", "-o" -> {
                    if (i + 1 < args.length) {
                        outputPath = args[i + 1];
                        i++; // Skip the next argument as we've used it
                    } else {
                        System.err.println("Error: Missing file path after --output/-o option");
                        System.out.println(Entry.HELP);
                        System.exit(1);
                    }
                }
                case "--version" -> {
                    System.out.println("Deco Compiler v" + VERSION);
                    System.exit(0);
                }
                case "--help", "-h" -> {
                    System.out.println(Entry.HELP);
                    System.exit(0);
                }
                default -> {
                    // If it's not a known option and no input path set yet, treat it as input path
                    if (!args[i].startsWith("-") && srcPath == null) {
                        srcPath = args[i];
                    } else if (args[i].startsWith("-")) {
                        System.err.println("Unknown option: " + args[i]);
                        System.out.println(Entry.HELP);
                        System.exit(1);
                    } else {
                        System.err.println("Error: Multiple input directories specified");
                        System.out.println(Entry.HELP);
                        System.exit(1);
                    }
                }
            }
        }
    }
}
