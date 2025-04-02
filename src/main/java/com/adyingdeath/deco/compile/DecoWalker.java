package com.adyingdeath.deco.compile;

import com.adyingdeath.deco.core.DecoFunctionRunner;
import com.adyingdeath.deco.datapack.decorator.Decorator;
import com.adyingdeath.deco.parser.DecoBaseListener;
import com.adyingdeath.deco.parser.DecoParser;

import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.datapack.Datapack;


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
        String locationStr = datapack.getCurrentFile().getNamespace()
                + ":"
                + datapack.getCurrentFile().getPath()
                + ctx.name.getText();
        Function function = new Function(locationStr);
        // Add all commands to the function
        ctx.blockStatement().statement().forEach((e) -> {
            if (e.MC_COMMAND() != null) {
                // This is MC command
                function.addCommand(e.getText().trim());
            } else if (e.expression() != null) {
                if (e.expression() instanceof DecoParser.FuncCallExprContext) {
                    DecoParser.FuncCallExprContext expr = (DecoParser.FuncCallExprContext) e.expression();
                    String functionName = expr.functionCall().name.getText();
                    DecoFunctionRunner.run(functionName, datapack, function, expr.functionCall().argumentList());
                }
            }
        });
        // Add the function to the sandbox
        datapack.addFunction(function);

        if (!ctx.functionDecorator().isEmpty()) {
            for (DecoParser.FunctionDecoratorContext decoratorObj : ctx.functionDecorator()) {
                Decorator decorator = this.datapack.decoratorLoader.getDecorator(decoratorObj.name.getText());
                if (decoratorObj.parameterList() != null) {
                    // The decorator has parameters
                    String[] parameters = decoratorObj.parameterList().parameter()
                            .stream().map((i) -> DecoUtil.processString(i.getText()))
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
