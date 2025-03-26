package com.adyingdeath.deco.compile;

import com.adyingdeath.deco.sandbox.Function;
import com.adyingdeath.deco.sandbox.Sandbox;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Compiler for generating Minecraft datapacks from Deco source files
 */
public class DatapackCompiler {
    private final String srcPath;
    private final String outputPath;
    private final Sandbox sandbox;
    private List<String> namespaces;
    private final Compiler compiler;
    
    // Default datapack format version (for pack.mcmeta)
    private static final int DEFAULT_PACK_FORMAT = 15; // for Minecraft 1.20.x
    private static final String DEFAULT_PACK_DESCRIPTION = "Generated by Deco Compiler";

    // File extension for Deco source files
    private static final String DECO_FILE_EXTENSION = ".deco";
    
    /**
     * Constructor for DatapackCompiler
     * 
     * @param srcPath The source folder with the same structure as a normal datapack
     * @param outputPath The output directory path for the generated datapack
     */
    public DatapackCompiler(String srcPath, String outputPath) {
        this.srcPath = srcPath;
        this.outputPath = outputPath;
        this.sandbox = new Sandbox();
        this.compiler = new Compiler(this.sandbox);
    }
    
    /**
     * Compile the datapack
     * 
     * @return true if compilation was successful, false otherwise
     */
    public boolean compile() {
        try {
            // Read source datapack
            File dataDir = new File(this.srcPath, "data");
            File[] namespaceFiles = dataDir.listFiles();
            if (namespaceFiles == null) {
                System.out.println("There is nothing in the datapack. Compilation done.");
                return true;
            }

            // Get all the namespaces, and build a String array of their name.
            this.namespaces = Arrays.stream(namespaceFiles)
                    .filter((File::isDirectory))
                    .map(File::getName)
                    .toList();

            // Compile all the namespaces
            for (String namespace : this.namespaces) {
                this.compileNamespace(
                        namespace,
                        Paths.get(this.srcPath, "data", namespace, "functions").toFile(),
                        Paths.get("/"));
            }

            if (!createDatapackStructure()) {
                System.err.println("Failed to create datapack structure");
                return false;
            }

            // Write mcfunction files
            for (Function function : this.sandbox.getFunctions()) {
                File functionFile = Paths
                        .get(this.outputPath, "data", function.getNamespace(), "functions", function.getPath(), function.getName() + ".mcfunction")
                        .toFile();

                functionFile.getParentFile().mkdirs();
                functionFile.createNewFile();

                FileWriter writer = new FileWriter(functionFile);
                writer.write(String.join("\n", function.getCommands()));
                writer.close();
            }

            return true;
        } catch (Exception e) {
            System.err.println("Error during datapack compilation: " + e.getMessage());
            return false;
        }
    }

    /**
     * Recursively process files in the datapack
     *
     * @param namespace The namespace of the current file
     * @param currentFile The current file or directory being processed
     * @param relativePath The path relative to the namespace directory
     */
    private void compileNamespace(String namespace, File currentFile, Path relativePath) {
        if (currentFile.isDirectory()) {
            // Process all files in directory
            File[] files = currentFile.listFiles();
            if (files == null) {
                return;
            }

            for (File file : files) {
                Path newRelativePath = relativePath.resolve(file.getName());

                this.compileNamespace(namespace, file, newRelativePath);
            }
        } else {
            // Process single file
            String fileName = currentFile.getName();
            String[] filenameParts = fileName.split("\\.");

            if (filenameParts.length != 2) {
                return;
            }

            String baseName = filenameParts[0];
            String extension = filenameParts[1];

            if (extension.equals("deco") || extension.equals("mcfunction")) {
                try {
                    // Read file content
                    String content = Files.readString(currentFile.toPath());
                    
                    // Create DecoFile object
                    DecoFile decoFile = new DecoFile(namespace, 
                                                   relativePath.getParent() != null ? relativePath.getParent().toString() : "",
                                                   baseName, 
                                                   content);

                    // Compile the file using DecoFile
                    boolean result = compiler.compile(decoFile);
                    
                    if (!result) {
                        System.err.println("Failed to compile: " + currentFile.getPath());
                    }
                } catch (IOException e) {
                    System.err.println("Error processing file: " + currentFile.getPath());
                    e.printStackTrace();
                }
            }
        }
    }
    
    /**
     * Create the datapack directory structure
     * 
     * @return true if successful, false otherwise
     */
    private boolean createDatapackStructure() {
        try {
            // Create base directories
            File outputDir = new File(outputPath);
            // [datapack]/data
            File dataDir = new File(outputPath, "data");
            // [datapack]/data/minecraft
            File minecraftDir = new File(dataDir, "minecraft");

            File functionsDir = new File(minecraftDir, "functions");
            File tagsDir = new File(minecraftDir, "tags");
            File functionTagsDir = new File(tagsDir, "functions");
            
            // Create directories
            outputDir.mkdirs();
            dataDir.mkdirs();
            minecraftDir.mkdirs();
            functionsDir.mkdirs();
            tagsDir.mkdirs();
            functionTagsDir.mkdirs();


            // Copy pack.mcmeta
            File packMcmeta = new File(srcPath, "pack.mcmeta");
            File packMcmetaOutput = new File(outputPath, "pack.mcmeta");
            Files.copy(packMcmeta.toPath(), packMcmetaOutput.toPath(), java.nio.file.StandardCopyOption.REPLACE_EXISTING);
            
            return true;
        } catch (Exception e) {
            System.err.println("Error creating datapack structure: " + e.getMessage());
            return false;
        }
    }
}