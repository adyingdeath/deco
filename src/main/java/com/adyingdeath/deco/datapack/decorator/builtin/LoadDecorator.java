package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.ResourceLocation;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.decorator.Decorator;

public class LoadDecorator implements Decorator {
    @Override
    public String getName() {
        return "load";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        String location = function.getLocation().toString();
        datapack.getFunctionTag("minecraft:load").addValue(location);
    }
}
