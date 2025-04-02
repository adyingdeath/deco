package com.adyingdeath.deco.core;

import com.adyingdeath.deco.core.function.RaycastFunction;
import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.parser.DecoParser;

public class DecoFunctionRunner {
    public static void run(String name, Datapack datapack, Function function, DecoParser.ArgumentListContext arguments) {
        switch (name) {
            case "raycast" -> {
                new RaycastFunction().enterFunctionCall(datapack, function, arguments);
            }
        }
    }
}
