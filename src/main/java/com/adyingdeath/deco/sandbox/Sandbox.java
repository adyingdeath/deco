package com.adyingdeath.deco.sandbox;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

public class Sandbox {
    // Function List
    private final List<Function> functions;
    
    // The output directory for generated files
    private String outputDirectory;

    public Sandbox() {
        super();
        this.functions = new ArrayList<>();
        this.outputDirectory = "decoOutput";
    }
    
    /**
     * Set the output directory for generated mcfunction files
     * @param outputDirectory The directory path
     */
    public void setOutputDirectory(String outputDirectory) {
        if (outputDirectory != null && !outputDirectory.isEmpty()) {
            this.outputDirectory = outputDirectory;
        }
    }

    /**
     * Add a function to the sandbox
     * @param function The function to add
     */
    public void addFunction(Function function) {
        if (function != null) {
            this.functions.add(function);
        }
    }

    /**
     * Get all functions in the sandbox
     * @return The list of functions
     */
    public List<Function> getFunctions() {
        return this.functions;
    }
    
    /**
     * Generate all mcfunction files for the functions in the sandbox
     * @throws IOException If an error occurs during file generation
     */
    public void generateMcFunctions() throws IOException {
        if (functions.isEmpty()) {
            System.out.println("No functions to generate");
            return;
        }
        
        // Prepare output directory
        Path outputPath = Paths.get(outputDirectory);
        if (!Files.exists(outputPath)) {
            Files.createDirectories(outputPath);
        }
        
        // Generate each function
        for (Function function : functions) {
            generateMcFunction(function);
        }
        
        System.out.println("Generated " + functions.size() + " mcfunction files in " + outputDirectory);
    }
    
    /**
     * Generate an mcfunction file for a single function
     * @param function The function to generate
     * @throws IOException If an error occurs during file generation
     */
    private void generateMcFunction(Function function) throws IOException {
        // Prepare the function path
        String namespace = function.getNamespace();
        String path = function.getPath();
        String name = function.getName();
        
        // Create the directory structure
        Path functionDir = Paths.get(outputDirectory, "data", namespace, path);
        if (!Files.exists(functionDir)) {
            Files.createDirectories(functionDir);
        }
        
        // Generate the function file
        Path functionFile = functionDir.resolve(name + ".mcfunction");
        try (FileWriter writer = new FileWriter(functionFile.toFile())) {
            writer.write(function.generateContent());
        }
        
        System.out.println("Generated function: " + function.getFullPath());
    }

    public void run() {
        System.out.println("Hello, World!");
    }
}


