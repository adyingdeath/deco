using System.Collections.Generic;

namespace Deco.Compiler.Expressions
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> _symbols = new();
        private readonly SymbolTable _parent;

        public SymbolTable(SymbolTable parent = null)
        {
            _parent = parent;
        }

        public bool Add(Symbol symbol)
        {
            return _symbols.TryAdd(symbol.Name, symbol);
        }

        public Symbol Get(string name)
        {
            if (_symbols.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            return _parent?.Get(name);
        }
    }
}
