package com.adyingdeath.deco.datapack.decorator;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.Function;

import java.util.Map;

/**
 * Interface for function decorators
 */
public interface Decorator {
    /**
     * Get the name of this decorator (without @ symbol)
     * @return Decorator name
     */
    String getName();

    /**
     * Apply the decorator to a function
     * @param params Parameters passed to the decorator
     * @param function The function to decorate
     * @param datapack The datapack
     */
    void apply(String[] params, Function function, Datapack datapack);
}