package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.Function;
import com.adyingdeath.deco.datapack.decorator.Decorator;

import java.util.Map;

public class TickDecorator implements Decorator {
    @Override
    public String getName() {
        return "tick";
    }

    @Override
    public void apply(Map<String, Object> params, Function function, Datapack datapack) {

    }
}
