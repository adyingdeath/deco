namespace Deco.Types;

/// <summary>
/// Represents a symbol in the symbol table.
/// </summary>
public class Symbol(
    string name, Type type, SymbolKind kind,
    int line = 0, int column = 0
) {
    public string Name { get; } = name;
    public Type Type { get; } = type;
    public SymbolKind Kind { get; } = kind;
    public int Line { get; } = line;
    public int Column { get; } = column;
}

/// <summary>
/// Kind of symbol.
/// </summary>
public enum SymbolKind {
    Variable,
    Function,
    Parameter,
}

/// <summary>
/// Represents a symbol table (originally called scope).
/// Symbol tables are nested hierarchically.
/// </summary>
public class Scope(string name, Scope? parent = null) {
    public Scope? Parent { get; } = parent;
    public List<Scope> Children { get; } = [];
    public string Name { get; } = name;
    public Dictionary<string, Symbol> Symbols { get; } = [];

    /// <summary>
    /// Adds a symbol to this symbol table.
    /// Throws exception if symbol already exists in this table.
    /// </summary>
    public void AddSymbol(Symbol symbol) {
        if (Symbols.TryGetValue(symbol.Name, out Symbol? value)) {
            throw new SymbolTableException(
                $"Symbol '{symbol.Name}' already declared in symbol table '{Name}' at line {value.Line}.",
                symbol.Line, symbol.Column
            );
        }
        Symbols[symbol.Name] = symbol;
    }

    /// <summary>
    /// Looks up a symbol starting from this symbol table and up the parent chain.
    /// Returns null if not found.
    /// </summary>
    public Symbol? LookupSymbol(string name) {
        if (Symbols.TryGetValue(name, out var symbol)) {
            return symbol;
        }
        return Parent?.LookupSymbol(name);
    }

    public Scope CreateChild(string name) {
        var child = new Scope(name, this);
        Children.Add(child);
        return child;
    }
}

/// <summary>
/// This class is used to enter and leave scope chain. PushScope and PopScope
/// should be used in pair. The PushScope method can take a null parameter,
/// which will take no effect, and its coresponding PopScope won't truly pop
/// scope from stack but will still return the last scope in stack.
/// </summary>
/// <param name="init"></param>
public class ScopeStack(Scope init) {
    private readonly List<Scope> _stack = [init];
    /// <summary>
    /// Variable to count the times the PushScope receive null paramter.
    /// If PushScope and PopScope are used in pair, we can garantee that the
    /// null Push will not lead to wrong Pop.
    /// </summary>
    private int _nullCounter = 0;

    public void PushScope(Scope? scope) {
        if (scope == null) {
            _nullCounter++;
            return;
        }
        _stack.Add(scope);
    }
    public Scope PopScope() {
        var scope = _stack[^1];
        if (_nullCounter > 0) {
            _nullCounter--;
        } else if (_stack.Count > 1) {
            _stack.RemoveAt(_stack.Count - 1);
        }
        return scope;
    }
    public Scope Current() {
        return _stack[^1];
    }
    public Scope Root() {
        return _stack[0];
    }
}

/// <summary>
/// Exception thrown when symbol table errors occur.
/// </summary>
public class SymbolTableException(
    string message, int line = 0, int column = 0
) : Exception(message) {
    public int Line { get; } = line;
    public int Column { get; } = column;
}
