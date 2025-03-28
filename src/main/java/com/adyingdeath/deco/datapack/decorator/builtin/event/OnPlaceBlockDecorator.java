package com.adyingdeath.deco.datapack.decorator.builtin.event;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.tags.FunctionTag;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class OnPlaceBlockDecorator implements Decorator {
    static final String template = """
        {
            "criteria": {
                "deco_on_place_block": {
                    "trigger": "minecraft:placed_block",
                    "conditions": {
                        "location": [
                            {
                                "condition": "minecraft:block_state_property",
                                "block": "minecraft:white_wool"
                            }
                        ]
                    }
                }
            },
            "rewards": {
                "function": "<PLACE_HOLDER>"
            }
        }
        """;
    @Override
    public String getName() {
        return "onPlaceBlock";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        JsonObject advancement = JsonParser.parseString(template).getAsJsonObject();
        String functionLocation = DatapackUtil.standardizeResourceLocation(
                function.getNamespace(), function.getFullPath()
        );
        // Set rewards.function to the decorated function
        advancement.getAsJsonObject("rewards").addProperty("function", functionLocation);

        datapack.advancementManager.addAdvancement("deco:place_block", advancement);
    }
}
