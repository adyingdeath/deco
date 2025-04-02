package com.adyingdeath.deco.core.function;

import com.adyingdeath.deco.core.DecoFunction;
import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.parser.DecoParser;

public class RaycastFunction extends DecoFunction {
    public void enterFunctionCall(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments) {
        String shooter = arguments.expression(0).getText();
        String direction = arguments.expression(1).getText();
        String callback = arguments.expression(2).getText();

        Function hitBlock = new Function("deco:raycast/hit_block");
        hitBlock.addCommand("scoreboard players set #hit vdvcasttemp 1");
        datapack.function.add(hitBlock);

        function.addCommand("execute as <shooter> at @s anchored eyes positioned ^ ^ ^ anchored feet run function vdv_raycast:start_ray".replace("<shooter>", shooter));
    }
    public void leaveFunction(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments) {

    }
}
