package com.adyingdeath.deco.core;

import com.adyingdeath.deco.datapack.Datapack;
import com.adyingdeath.deco.datapack.function.Function;
import com.adyingdeath.deco.parser.DecoParser;

public abstract class DecoFunction {
    public abstract void enterFunctionCall(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments);
    public abstract void leaveFunction(Datapack datapack, Function function, DecoParser.ArgumentListContext arguments);
}
