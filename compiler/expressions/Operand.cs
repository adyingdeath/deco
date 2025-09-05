namespace Deco.Compiler.Expressions {
    public abstract class Operand { }

    public class ConstantOperand : Operand {
        public string Value { get; }
        public string Type { get; }

        public ConstantOperand(string value, string type) {
            Value = value;
            Type = type;
        }
    }

    public class SymbolOperand : Operand {
        public Symbol Symbol { get; }

        public SymbolOperand(Symbol symbol) {
            Symbol = symbol;
        }
    }
}
