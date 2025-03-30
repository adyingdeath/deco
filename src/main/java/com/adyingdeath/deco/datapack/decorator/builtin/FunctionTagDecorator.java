package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.ResourceLocation;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.tags.FunctionTag;
import com.adyingdeath.deco.datapack.decorator.Decorator;

public class FunctionTagDecorator implements Decorator {
    @Override
    public String getName() {
        return "tag";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        if (params.length != 1 || params[0].isEmpty()) return;
        ResourceLocation tagLocation = new ResourceLocation(params[0]);
        String location = function.getLocation().toString();
        // Create tag file if not existed
        if (datapack.getFunctionTag(tagLocation.toString()) == null) {
            datapack.addFunctionTag(
                    tagLocation.toString(),
                    new FunctionTag().addValue(location)
            );
        } else {
            datapack.getFunctionTag("minecraft:load").addValue(location);
        }
    }
}
