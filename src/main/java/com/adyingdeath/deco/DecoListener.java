package com.adyingdeath.deco;

import com.adyingdeath.deco.parser.DecoBaseListener;
import com.adyingdeath.deco.parser.DecoParser;
import java.util.stream.Collectors;


public class DecoListener extends DecoBaseListener {
    @Override
    public void exitFunction(DecoParser.FunctionContext ctx) {
        System.out.println("Function: " + ctx.name.getText());
        System.out.println("Body: " + ctx.blockStatement().statement().stream().map((e) -> {
            return e.getText();
        }).collect(Collectors.joining(" ")));
    }
}
