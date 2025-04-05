package com.adyingdeath.deco.datapack.decorator.builtin.event;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.datapack.function.Function;

public class CarrotOnAStickDecorator implements Decorator {

    @Override
    public String getName() {
        return "carrotOnAStick";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        if (datapack.memory.get("carrot_on_a_stick") == null) {
            // If the carrot on a stick is not in the memory, it means it's the first time the event is used. So we need to setup stuff for the first time, like creating the scoreboard and so on.

            // Create a function where we will put all future carrot on a stick events' callbacks
            Function carrotCallback = new Function("deco:core/carrot" + DatapackUtil.randomCode(8));
            carrotCallback.addCommand("scoreboard players set @s deco.carrotonastick 0");
            datapack.function.add(carrotCallback);
            // Create the scoreboard
            datapack.addLoad("scoreboard objectives add deco.carrotonastick minecraft.used:minecraft.carrot_on_a_stick");
            // Execute the function every time a player uses the carrot on a stick(as the player)
            datapack.addTick("execute as @e[scores={deco.carrotonastick=1..}] run function " + carrotCallback.getLocation().toString());

            // Set the memory so that we don't need to do the setup again when this decorator is used again
            datapack.memory.put("carrot_on_a_stick", carrotCallback);
            carrotCallback.insertCommand(-1,"execute as @s at @s run function " + function.getLocation().toString());
            return;
        }
        // If the carrot on a stick is already in the memory, it means it's not the first time the event is used. So we don't need to do the setup again.
        // We just need to get the callback function and put our new function into it.
        Function carrotCallback = (Function) datapack.memory.get("carrot_on_a_stick");
        carrotCallback.insertCommand(-1, "execute as @s at @s run function " + function.getLocation().toString());
    }
}
