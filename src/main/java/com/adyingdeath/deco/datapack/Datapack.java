package com.adyingdeath.deco.datapack;

import com.adyingdeath.deco.compile.DatapackCompiler;
import com.adyingdeath.deco.compile.DecoFile;

import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

public class Datapack {
    private DecoFile currentFile;
    // Function List
    private final List<Function> functions;
    private final Map<String, Path> functionTags;
    private final DatapackCompiler compiler;

    public Datapack(DatapackCompiler compiler) {
        this.functions = new ArrayList<>();
        this.compiler = compiler;
    }

    public void loadFunctionTags () {
        for (String namespace : this.compiler.getNamespaces()) {
            Path functionTagsPath = Paths.get(this.compiler.getSrcPath(), "data", namespace, "function_tags.json");
            if (Files.exists(functionTagsPath)) {
                this.functionTags.put(namespace, functionTagsPath);
            }
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


