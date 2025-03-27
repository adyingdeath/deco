package com.adyingdeath.deco.datapack.decorator;

import java.io.File;
import java.io.IOException;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;

/**
 * Responsible for loading decorator implementations
 */
public class DecoratorLoader {
    private final Map<String, Decorator> decorators = new HashMap<>();

    /**
     * Constructor for the DecoratorLoader
     */
    public DecoratorLoader() {
        // Register built-in decorators
        registerBuiltinDecorators();
    }

    /**
     * Register the default built-in decorators
     */
    private void registerBuiltinDecorators() {
        // Register built-in decorators
        /* registerDecorator(new TickDecorator());
        registerDecorator(new LoadDecorator());
        registerDecorator(new PrivateDecorator()); */
    }

    /**
     * Register a new decorator
     * @param decorator The decorator to register
     */
    public void registerDecorator(Decorator decorator) {
        decorators.put(decorator.getName(), decorator);
    }

    /**
     * Get a decorator by name
     * @param name The name of the decorator
     * @return The decorator or null if not found
     */
    public Decorator getDecorator(String name) {
        return decorators.get(name);
    }
}
