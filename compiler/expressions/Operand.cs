namespace Deco.Compiler.Expressions
{
    public abstract class Operand { }

    public class ConstantOperand : Operand
    {
        public string Value { get; }
        public SymbolType Type { get; }

        public ConstantOperand(string value, SymbolType type)
        {
            Value = value;
            Type = type;
        }
    }

    public class SymbolOperand : Operand
    {
        public Symbol Symbol { get; }

        public SymbolOperand(Symbol symbol)
        {
            Symbol = symbol;
        }
    }
}
