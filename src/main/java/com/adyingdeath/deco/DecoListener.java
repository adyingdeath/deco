package com.adyingdeath.deco;

import com.adyingdeath.deco.parser.DecoBaseListener;
import com.adyingdeath.deco.parser.DecoParser;

import java.util.stream.Collectors;

import com.adyingdeath.deco.sandbox.Function;
import com.adyingdeath.deco.sandbox.Sandbox;


public class DecoListener extends DecoBaseListener {
    private Sandbox sandbox;

    public DecoListener(Sandbox sandbox) {
        super();
        this.sandbox = sandbox;
    }

    /**
     * Called when the parser exits a function context. Add the function to the sandbox.
     * So that the function can be written to a mcfunction file later.
     * @param ctx The context of the function
     */
    @Override
    public void exitFunction(DecoParser.FunctionContext ctx) {
        Function function = new Function(ctx.name.getText());
        // Set function decorator if it exists
        if (ctx.function_decorator() != null) {
            function.setDecorator(ctx.function_decorator().name.getText());
        }
        // Add all commands to the function
        ctx.blockStatement().statement().forEach((e) -> {
            if (e.MC_COMMAND() != null) {
                function.addCommand(e.getText());
            }
        });
        // Add the function to the sandbox
        sandbox.addFunction(function);
    }
}
