package com.adyingdeath.deco.sandbox;

import com.adyingdeath.deco.compile.DecoFile;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

public class Sandbox {
    private DecoFile currentFile;
    // Function List
    private final List<Function> functions;

    public Sandbox() {
        this.functions = new ArrayList<>();
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


