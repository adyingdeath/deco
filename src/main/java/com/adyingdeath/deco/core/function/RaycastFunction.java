package com.adyingdeath.deco.core.function;

import com.adyingdeath.deco.compile.DecoUtil;
import com.adyingdeath.deco.core.DecoFunction;
import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.parser.DecoParser;

public class RaycastFunction extends DecoFunction {
    public void enterFunctionCall(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments) {
        String shooter = DecoUtil.processString(arguments.expression(0).getText());
        String block = DecoUtil.processString(arguments.expression(1).getText());
        float step = Float.parseFloat(arguments.expression(2).getText());
        float distance = Float.parseFloat(arguments.expression(3).getText());
        String callback = DecoUtil.processString(arguments.expression(4).getText());

        Function hitBlock = new Function("deco:raycast/hit_block");
        hitBlock.addCommand("scoreboard players set deco.9rjyi591 deco.raycast 1\n" + callback);
        datapack.function.add(hitBlock);

        Function ray = new Function("deco:raycast/ray");
        ray.addCommand("""
                execute if block ~ ~ ~ <block> run function deco:raycast/hit_block
                scoreboard players add deco.56y56u63 deco.raycast 1
                execute if score deco.9rjyi591 deco.raycast matches 0 if score deco.56y56u63 deco.raycast matches ..<limit> positioned ^ ^ ^<step> run function deco:raycast/ray
                """
                .replace("<block>", block)
                .replace("<limit>", String.valueOf((int) (distance / step)))
                .replace("<step>", String.valueOf(step)));
        datapack.function.add(ray);

        Function startRay = new Function("deco:raycast/start_ray");
        startRay.addCommand("""
                tag @s add vdvray
                scoreboard players set deco.9rjyi591 deco.raycast 0
                scoreboard players set deco.56y56u63 deco.raycast 0
                function deco:raycast/ray
                tag @s remove vdvray
                """);
        datapack.function.add(startRay);

        datapack.addLoad("scoreboard objectives add deco.raycast dummy");

        function.addCommand("execute as <shooter> at @s anchored eyes positioned ^ ^ ^ anchored feet run function deco:raycast/start_ray".replace("<shooter>", shooter));
    }
    public void leaveFunction(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments) {

    }
}
