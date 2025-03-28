package com.adyingdeath.deco.datapack.tags;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.annotations.SerializedName;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a Minecraft function tag JSON file
 * Format example:
 * {
 *   "values": [
 *     "namespace:path/to/function"
 *   ],
 *   "replace": true
 * }
 */
public class FunctionTag {
    /**
     * List of function references in the tag
     */
    @SerializedName("values")
    private List<String> values;
    
    /**
     * Whether this tag replaces an existing tag
     */
    @SerializedName("replace")
    private boolean replace;
    
    // Gson instance for serialization/deserialization
    private static final Gson GSON = new GsonBuilder().setPrettyPrinting().create();
    
    /**
     * Create a new empty function tag
     */
    public FunctionTag() {
        this.values = new ArrayList<>();
        this.replace = false;
    }
    
    /**
     * Create a function tag with initial values
     * 
     * @param values List of function references
     * @param replace Whether this tag replaces existing ones
     */
    public FunctionTag(List<String> values, boolean replace) {
        this.values = values != null ? values : new ArrayList<>();
        this.replace = replace;
    }
    
    /**
     * Add a function reference to this tag
     * 
     * @param functionRef The function reference to add (format: "namespace:path/to/function")
     * @return This FunctionTag instance for method chaining
     */
    public FunctionTag addValue(String functionRef) {
        if (functionRef != null && !functionRef.isEmpty()) {
            this.values.add(functionRef);
        }
        return this;
    }
    
    /**
     * Set whether this tag replaces existing tags
     * 
     * @param replace True if this tag should replace existing tags
     * @return This FunctionTag instance for method chaining
     */
    public FunctionTag setReplace(boolean replace) {
        this.replace = replace;
        return this;
    }
    
    /**
     * Get the list of function references
     * 
     * @return List of function references
     */
    public List<String> getValues() {
        return this.values;
    }
    
    /**
     * Check if this tag replaces existing tags
     * 
     * @return True if this tag replaces existing tags
     */
    public boolean isReplace() {
        return this.replace;
    }
    
    /**
     * Convert this function tag to a JSON string
     * 
     * @return JSON string representation
     */
    public String toJson() {
        return GSON.toJson(this);
    }
    
    /**
     * Parse a JSON string into a FunctionTag
     * 
     * @param json JSON string to parse
     * @return FunctionTag instance
     * @throws com.google.gson.JsonSyntaxException If the JSON is invalid
     */
    public static FunctionTag fromJson(String json) {
        if (json == null || json.trim().isEmpty()) {
            return new FunctionTag();
        }
        return GSON.fromJson(json, FunctionTag.class);
    }
} 