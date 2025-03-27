package com.adyingdeath.deco.datapack;

import com.adyingdeath.deco.compile.DatapackCompiler;
import com.adyingdeath.deco.compile.DecoFile;
import com.adyingdeath.deco.datapack.decorator.DecoratorLoader;

import java.io.IOException;
import java.nio.file.*;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Consumer;

public class Datapack {
    private DecoFile currentFile;
    // Function List
    private final List<Function> functions;
    private final Map<String, String> functionTags;
    private final DatapackCompiler compiler;
    public final DecoratorLoader decoratorLoader;

    public Datapack(DatapackCompiler compiler) {
        this.functions = new ArrayList<>();
        this.functionTags = new HashMap<>();
        this.compiler = compiler;
        this.decoratorLoader = compiler.decoratorLoader;

        this.loadFunctionTags();
    }
    
    public void loadFunctionTags () {
        try {
            for (String namespace : this.compiler.getNamespaces()) {
                Path start = Paths.get(
                        this.compiler.getSrcPath().toString(),
                        "data", namespace, "tags", "functions");
                if (!Files.exists(start)) continue;
                Files.walkFileTree(start, new SimpleFileVisitor<Path>() {
                    @Override
                    public FileVisitResult visitFile(Path file, BasicFileAttributes attrs) throws IOException {
                        // Calculate path string
                        Path relative = start.relativize(file);
                        String fileName = relative.getFileName().toString();
                        String baseName = fileName.substring(0, fileName.lastIndexOf('.'));
                        Path relativePath = relative.getParent() != null ? relative.getParent().resolve(baseName) : Paths.get(baseName);
                        String resourceLocation = namespace + ":" + relativePath.toString();
                        functionTags.put(resourceLocation, Files.readString(file));
                        return FileVisitResult.CONTINUE;
                    }
                });
            }
            System.out.println(1);
        } catch (IOException e) {
            System.out.println("Error: Fail to load tags");
            return;
        }

    }
    /**
     * Set the current file
     * @param currentFile The file to set
     */
    public void setCurrentFile(DecoFile currentFile) {
        this.currentFile = currentFile;
    }

    /**
     * Get the current file
     * @return The current file
     */
    public DecoFile getCurrentFile() {
        return this.currentFile;
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
}


