package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.Function;
import com.adyingdeath.deco.datapack.decorator.Decorator;

import java.util.Map;

public class LoadDecorator implements Decorator {
    @Override
    public String getName() {
        return "load";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        String location = DatapackUtil.standardizeResourceLocation(function.getNamespace(), function.getFullPath());
        datapack.getFunctionTag("minecraft:load").addValue(location);
    }
}
