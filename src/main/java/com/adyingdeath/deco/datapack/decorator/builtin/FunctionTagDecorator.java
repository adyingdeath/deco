package com.adyingdeath.deco.datapack.decorator.builtin;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.DatapackUtil;
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
        String tag = params[0];
        String[] tagSplit = tag.split(":");
        if (tagSplit.length != 2) return;
        String location = DatapackUtil.standardizeResourceLocation(function.getNamespace(), function.getFullPath());
        // Create tag file if not existed
        if (datapack.getFunctionTag(tag) == null) {
            datapack.addFunctionTag(
                DatapackUtil.standardizeResourceLocation(tagSplit[0], tagSplit[1]),
                new FunctionTag().addValue(location)
            );
        } else {
            datapack.getFunctionTag("minecraft:load").addValue(location);
        }
    }
}
