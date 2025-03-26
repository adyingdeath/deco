package com.adyingdeath.deco.compile;

import com.adyingdeath.deco.parser.DecoBaseListener;
import com.adyingdeath.deco.parser.DecoParser;

import com.adyingdeath.deco.sandbox.Function;
import com.adyingdeath.deco.sandbox.Sandbox;
import org.antlr.v4.runtime.tree.TerminalNode;


public class DecoWalker extends DecoBaseListener {
    private Sandbox sandbox;

    public DecoWalker(Sandbox sandbox) {
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
        Function function = new Function(ctx.name.getText())
                .setNamespace(sandbox.getCurrentFile().getNamespace())
                .setPath(sandbox.getCurrentFile().getPath());
        // Add all commands to the function
        ctx.blockStatement().statement().forEach((e) -> {
            if (e.MC_COMMAND() != null) {
                function.addCommand(e.getText().trim());
            }
        });
        // Add the function to the sandbox
        sandbox.addFunction(function);
    }

    @Override
    public void exitProgram(DecoParser.ProgramContext ctx) {
        Function function = new Function(sandbox.getCurrentFile().getFilename())
                .setNamespace(sandbox.getCurrentFile().getNamespace())
                .setPath(sandbox.getCurrentFile().getPath());
        ctx.statement().forEach((stat) -> {
            if (stat.MC_COMMAND() != null) {
                function.addCommand(stat.getText().trim());
            }
        });
        if (!function.getCommands().isEmpty()) {
            sandbox.addFunction(function);
        }
    }
}
