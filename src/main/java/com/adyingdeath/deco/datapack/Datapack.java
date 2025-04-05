package com.adyingdeath.deco.datapack;

import com.adyingdeath.deco.compile.DatapackCompiler;
import com.adyingdeath.deco.compile.DecoFile;
import com.adyingdeath.deco.datapack.advancement.Advancement;
import com.adyingdeath.deco.datapack.decorator.DecoratorLoader;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.tags.FunctionTag;

import java.io.IOException;
import java.nio.file.*;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Datapack {
    private DecoFile currentFile;
    // Function List
    public final List<Function> function;
    // Function Tags Map (resource location -> FunctionTag)
    private final Map<String, FunctionTag> functionTags;
    private final DatapackCompiler compiler;
    public final DecoratorLoader decoratorLoader;

    public final Advancement advancement;
    public final Map<String, Object> memory;

    // Add default load and tick functions
    private Function loadFunction = new Function("deco:core/load");
    private Function tickFunction = new Function("deco:core/tick");

    public Datapack(DatapackCompiler compiler) {
        this.function = new ArrayList<>();
        this.functionTags = new HashMap<>();
        this.compiler = compiler;
        this.decoratorLoader = compiler.decoratorLoader;
        this.advancement = new Advancement();
        this.memory = new HashMap<>();

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
                        
                        // Parse JSON content into FunctionTag
                        String jsonContent = Files.readString(file);
                        FunctionTag functionTag = FunctionTag.fromJson(jsonContent);
                        functionTags.put(resourceLocation, functionTag);
                        
                        return FileVisitResult.CONTINUE;
                    }
                });
            }
            // Default create tick and load
            if(this.getFunctionTag("minecraft:tick") == null) {
                this.functionTags.put("minecraft:tick", new FunctionTag());
            }
            if(this.getFunctionTag("minecraft:load") == null) {
                this.functionTags.put("minecraft:load", new FunctionTag());
            }
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
     * Register load and tick functions if they have commands
     */
    public void registerLoadTick() {
        if (!this.loadFunction.getCommands().isEmpty()) {
            this.function.add(this.loadFunction);
            FunctionTag tag = this.functionTags.get("minecraft:load");
            tag.addValue("deco:core/load");
        }
        if (!this.tickFunction.getCommands().isEmpty()) {
            this.function.add(this.tickFunction);
            FunctionTag tag = this.functionTags.get("minecraft:tick");
            tag.addValue("deco:core/tick");
        }
    }

    /**
     * Add a command to the deco core load function.
     * This is used to enable programmers to easily add commands which need to be executed when the datapack is loaded.
     * @param command The command to add
     */
    public void addLoad(String command) {
        this.loadFunction.addCommand(command);
    }

    /**
     * Add a command to the deco core tick function
     * This is used to enable programmers to easily add commands which need to be executed every tick.
     * @param command The command to add
     */
    public void addTick(String command) {
        this.tickFunction.addCommand(command);
    }
    
    /**
     * Add a function to the sandbox
     * @param function The function to add
     */
    public void addFunction(Function function) {
        if (function != null) {
            this.function.add(function);
        }
    }

    /**
     * Get all functions in the sandbox
     * @return The list of functions
     */
    public List<Function> getFunction() {
        return this.function;
    }
    
    /**
     * Add a function tag to the datapack
     * @param resourceLocation The resource location of the tag (namespace:path/to/tag)
     * @param functionTag The function tag to add
     */
    public void addFunctionTag(String resourceLocation, FunctionTag functionTag) {
        if (resourceLocation != null && !resourceLocation.isEmpty() && functionTag != null) {
            this.functionTags.put(resourceLocation, functionTag);
        }
    }
    
    /**
     * Get a function tag by resource location
     * @param resourceLocation The resource location of the tag (namespace:path/to/tag)
     * @return The function tag, or null if not found
     */
    public FunctionTag getFunctionTag(String resourceLocation) {
        return this.functionTags.get(resourceLocation);
    }
    
    /**
     * Get all function tags in the datapack
     * @return Map of resource locations to function tags
     */
    public Map<String, FunctionTag> getFunctionTags() {
        return this.functionTags;
    }
}


