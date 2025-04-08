package com.adyingdeath.deco.datapack.decorator.builtin.event;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.advancement.Advancement;
import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.datapack.function.Function;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class OnPlayerDie implements Decorator {
    static final String template = """
        {
            "criteria": {
                "requirement": {
                    "trigger": "minecraft:entity_hurt_player",
                    "conditions": {
                        "player": [
                            {
                                "condition": "minecraft:entity_properties",
                                "entity": "this",
                                "predicate": {
                                    "nbt": "{Health:0f}"
                                }
                            }
                        ]
                    }
                }
            },
            "rewards": {
                "function": "<func>"
            }
        }
        """;
    @Override
    public String getName() {
        return "onPlayerDie";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        // Create an advancement to detect players' death
        JsonObject advancement = JsonParser.parseString(template.replace("<func>", function.getLocation().toString())).getAsJsonObject();
        String randomName = "deco:player_die_" + DatapackUtil.randomCode(8);
        datapack.advancement.addAdvancement(randomName, advancement);

        // Add command to revoke advancement
        function.addCommand("advancement revoke @s only " + randomName);
    }
}
