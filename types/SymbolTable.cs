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
/// Represents a scope in the symbol table.
/// </summary>
public class Scope(string name, Scope? parent = null) {
    public Scope? Parent { get; } = parent;
    public List<Scope> Children { get; } = [];
    public string Name { get; } = name;
    public Dictionary<string, Symbol> Symbols { get; } = [];

    /// <summary>
    /// Adds a symbol to this scope.
    /// Throws exception if symbol already exists in this scope.
    /// </summary>
    public void AddSymbol(Symbol symbol) {
        if (Symbols.TryGetValue(symbol.Name, out Symbol? value)) {
            throw new SymbolTableException(
                $"Symbol '{symbol.Name}' already declared in scope '{Name}' at line {value.Line}.",
                symbol.Line, symbol.Column
            );
        }
        Symbols[symbol.Name] = symbol;
    }

    /// <summary>
    /// Looks up a symbol starting from this scope and up the chain.
    /// Returns null if not found.
    /// </summary>
    public Symbol? LookupSymbol(string name) {
        if (Symbols.TryGetValue(name, out var symbol)) {
            return symbol;
        }
        return Parent?.LookupSymbol(name);
    }
}

/// <summary>
/// Main symbol table class.
/// </summary>
public class SymbolTable {
    public Scope GlobalScope { get; } = new Scope("global");
    private Scope _currentScope;

    public SymbolTable() {
        _currentScope = GlobalScope;
    }

    public void EnterScope(string name) {
        var newScope = new Scope(name, _currentScope);
        _currentScope.Children.Add(newScope);
        _currentScope = newScope;

    }

    public void ExitScope() {
        if (_currentScope.Parent != null) {
            _currentScope = _currentScope.Parent;
        } else {
            throw new InvalidOperationException("Cannot exit global scope");
        }
    }

    public Scope CurrentScope => _currentScope;

    /// <summary>
    /// Adds a symbol to the current scope.
    /// </summary>
    public void AddSymbol(Symbol symbol) {
        _currentScope.AddSymbol(symbol);
    }

    /// <summary>
    /// Looks up a symbol starting from current scope.
    /// </summary>
    public Symbol? LookupSymbol(string name) {
        return _currentScope.LookupSymbol(name);
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
