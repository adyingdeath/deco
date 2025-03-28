package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
import com.adyingdeath.deco.datapack.Function;
import com.adyingdeath.deco.datapack.decorator.Decorator;

import java.util.Map;

public class FunctionTagDecorator implements Decorator {
    @Override
    public String getName() {
        return "tag";
    }

    @Override
    public void apply(String[] params, Function function, Datapack datapack) {
        String location = DatapackUtil.standardizeResourceLocation(function.getNamespace(), function.getFullPath());
        datapack.getFunctionTag("minecraft:load").addValue(location);
    }
}
