package com.adyingdeath.deco.compile;

import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.parser.DecoBaseListener;
import com.adyingdeath.deco.parser.DecoParser;

import com.adyingdeath.deco.datapack.Function;
import com.adyingdeath.deco.datapack.Datapack;
import org.antlr.v4.runtime.tree.ParseTree;


public class DecoWalker extends DecoBaseListener {
    private Datapack datapack;

    public DecoWalker(Datapack datapack) {
        super();
        this.datapack = datapack;
    }

    /**
     * Called when the parser exits a function context. Add the function to the sandbox.
     * So that the function can be written to a mcfunction file later.
     * @param ctx The context of the function
     */
    @Override
    public void exitFunction(DecoParser.FunctionContext ctx) {
        Function function = new Function(ctx.name.getText())
                .setNamespace(datapack.getCurrentFile().getNamespace())
                .setPath(datapack.getCurrentFile().getPath());
        // Add all commands to the function
        ctx.blockStatement().statement().forEach((e) -> {
            if (e.MC_COMMAND() != null) {
                function.addCommand(e.getText().trim());
            }
        });
        // Add the function to the sandbox
        datapack.addFunction(function);

        if (!ctx.function_decorator().isEmpty()) {
            for (DecoParser.Function_decoratorContext decoratorObj : ctx.function_decorator()) {
                Decorator decorator = this.datapack.decoratorLoader.getDecorator(decoratorObj.name.getText());
                if (decoratorObj.parameterList() != null) {
                    // The decorator has parameters
                    String[] parameters = decoratorObj.parameterList().STRING()
                            .stream().map((i) -> {
                                String str = i.getText();
                                return str.substring(1, str.length() - 1);
                            })
                            .toArray(String[]::new);
                    
                    decorator.apply(parameters, function, this.datapack);
                } else {
                    decorator.apply(null, function, this.datapack);
                }
            }
        }
    }

    @Override
    public void exitProgram(DecoParser.ProgramContext ctx) {
        Function function = new Function(datapack.getCurrentFile().getFilename())
                .setNamespace(datapack.getCurrentFile().getNamespace())
                .setPath(datapack.getCurrentFile().getPath());
        ctx.statement().forEach((stat) -> {
            if (stat.MC_COMMAND() != null) {
                function.addCommand(stat.getText().trim());
            }
        });
        if (!function.getCommands().isEmpty()) {
            datapack.addFunction(function);
        }
    }
}
