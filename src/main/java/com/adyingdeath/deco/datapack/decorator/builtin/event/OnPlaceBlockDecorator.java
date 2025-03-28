package com.adyingdeath.deco.datapack.decorator.builtin.event;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.tags.FunctionTag;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class OnPlaceBlockDecorator implements Decorator {
    static final String template = """
        {
            "criteria": {
                "on_place_block": {
                    "trigger": "minecraft:placed_block",
                    "conditions": {
                        "location": [
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

        // Extract blocks from params if existed, and apply them to the advancement
        if (params != null) {
            JsonArray location = advancement
                    .getAsJsonObject("criteria")
                    .getAsJsonObject("on_place_block")
                    .getAsJsonObject("conditions")
                    .getAsJsonArray("location");

            for (String param : params) {
                JsonObject obj = new JsonObject();
                obj.addProperty("condition", "minecraft:block_state_property");
                obj.addProperty("block", param);
                location.add(obj);
            }
        }

        String randomName = "deco:place_block_" + DatapackUtil.randomCode(8);

        datapack.advancementManager.addAdvancement(randomName, advancement);
        function.addCommand("advancement revoke @s only " + randomName);
    }
}
