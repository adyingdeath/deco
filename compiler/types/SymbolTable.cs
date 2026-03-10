using System.Reflection;
using Deco.Compiler.Diagnostics.Errors;
using Deco.Compiler.Lib.Api;
using Deco.Compiler.IR;

namespace Deco.Compiler.Types;

/// <summary>
/// Represents a symbol in the symbol table.
/// </summary>
public class Symbol(
    string name, string code, IType type, SymbolKind kind,
    int line = 0, int column = 0
) {
    public string Name { get; } = name;
    public string Code { get; set; } = code;
    public IType Type { get; set; } = type;
    public SymbolKind Kind { get; } = kind;
    public int Line { get; } = line;
    public int Column { get; } = column;

    public static readonly Symbol Uninitialized = new(
        "__uninitialized__",
        "#",
        TypeUtils.UnknownType,
        SymbolKind.Variable,
        0, 0
    );
}

public class FunctionSymbol(
    string name,
    string code,
    IType type,
    List<Symbol> parameterSymbol,
    Symbol returnSymbol,
    int line = 0, int column = 0
) : Symbol(
    name, code, type, SymbolKind.Function,
    line, column
) {
    public List<Symbol> ParameterSymbol { get; set; } = parameterSymbol;
    public Symbol ReturnSymbol { get; set; } = returnSymbol;
}

/// <summary>
/// Represents a library function imported via reflection.
/// These functions generate IR directly rather than having a fixed body.
/// </summary>
public class LibraryFunctionSymbol(
    string name,
    string code,
    IType type,
    List<Symbol> parameterSymbol,
    Symbol returnSymbol,
    MethodInfo method,
    int line = 0, int column = 0
    ) : FunctionSymbol(name, code, type, parameterSymbol, returnSymbol, line, column) {
    /// <summary>
    /// The backing C# method info.
    /// </summary>
    public MethodInfo Method { get; } = method;

    /// <summary>
    /// A delegate that handles the invocation of the library function during IR generation.
    /// </summary>
    public Action<LibraryContext, List<Operand>, Operand?> Generator { get; } = (ctx, args, ret) => {
        // Prepare arguments for the C# method invocation.
        // Signature: (LibraryContext, Operand arg1, Operand arg2, ...)
        var invokeArgs = new object[args.Count + 1];
        invokeArgs[0] = ctx;
        for (int i = 0; i < args.Count; i++) {
            invokeArgs[i + 1] = args[i];
        }

        // Invoke the method
        var result = method.Invoke(null, invokeArgs);

        // Handle return value
        // If the C# method returns an Operand and we have a return slot (ret), move the result there.
        if (ret != null && result is Operand resultOp) {
            ctx.Emit(new MoveInstruction(resultOp, ret));
        }
    };
}

public enum SymbolKind {
    Variable,
    Function,
    Parameter,
}

/// <summary>
/// Represents a symbol table (originally called scope).
/// Symbol tables are nested hierarchically.
/// </summary>
public class Scope(CompilationContext context, string name, Scope? parent = null) {
    public Scope? Parent { get; } = parent;
    public List<Scope> Children { get; } = [];
    public string Name { get; } = name;
    public Dictionary<string, Symbol> Symbols { get; } = [];
    private readonly CompilationContext _context = context;

    /// <summary>
    /// Adds a symbol to this symbol table.
    /// Throws exception if symbol already exists in this table.
    /// </summary>
    public void AddSymbol(Symbol symbol) {
        if (Symbols.TryGetValue(symbol.Name, out Symbol? value)) {
            _context.ErrorReporter.Report(new DuplicateSymbolError(
                value, symbol, symbol.Line, symbol.Column
            ));
            return;
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
        var child = new Scope(_context, name, this);
        Children.Add(child);
        return child;
    }
}

/// <summary>
/// This class is used to enter and leave scope chain. PushScope and PopScope
/// should be used in pair. The PushScope method can take a null parameter,
/// which will take no effect, and its coresponding PopScope won't truly pop
/// scope from stack but will still return the last not-null scope in stack.
/// </summary>
/// <param name="root"></param>
public class ScopeStack(Scope root) {
    private readonly Stack<Scope> _stack = new([root]);
    /// <summary>
    /// List to mark all the points that the PushScope receive null paramter.
    /// If PushScope and PopScope are used in pair, we can garantee that the
    /// null Push will not lead to wrong Pop.
    /// </summary>
    private readonly Stack<int> _nullMarker = [];

    public void PushScope(Scope? scope) {
        if (scope == null) {
            _nullMarker.Push(_stack.Count);
        } else {
            _stack.Push(scope);
        }
    }
    public Scope PopScope() {
        if (_nullMarker.Count > 0 && _nullMarker.Peek() == _stack.Count) {
            _nullMarker.Pop();
            return _stack.Peek();
        } else if (_stack.Count > 1) {
            return _stack.Pop();
        }
        return root;
    }
    public Scope Current() => _stack.Peek();
    public Scope Root() => root;
}