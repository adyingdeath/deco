using System.Reflection;
using Deco.Compiler.Lib.Api;
using Deco.Compiler.Types;
using Deco.Compiler.Diagnostics.Errors;

namespace Deco.Compiler.Lib;

/// <summary>
/// Responsible for loading Deco libraries from assemblies using Reflection.
/// </summary>
public static class PluginLoader {
    /// <summary>
    /// Loads an external plugin assembly.
    /// </summary>
    public static void LoadPlugin(string assemblyPath, CompilationContext context, Scope globalScope) {
        if (!File.Exists(assemblyPath)) {
            Console.WriteLine($"[Warning] Plugin not found: {assemblyPath}");
            return;
        }
        var assembly = Assembly.LoadFrom(assemblyPath);
        LoadAssembly(assembly, context, globalScope);
    }

    /// <summary>
    /// Loads the standard library embedded in the current assembly.
    /// </summary>
    public static void LoadStandardLibrary(CompilationContext context, Scope globalScope) {
        LoadAssembly(Assembly.GetExecutingAssembly(), context, globalScope);
    }

    private static void LoadAssembly(Assembly assembly, CompilationContext context, Scope globalScope) {
        var types = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<DecoLibraryAttribute>() != null);

        foreach (var type in types) {
            // Optional: Initialize plugin if it implements IDecoPlugin
            if (typeof(IDecoPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface) {
                try {
                    var plugin = (IDecoPlugin)Activator.CreateInstance(type)!;
                    plugin.Initialize(context);
                } catch (Exception ex) {
                    Console.WriteLine($"[Error] Failed to initialize plugin {type.Name}: {ex.Message}");
                }
            }

            // Scan for static methods marked with DecoFunctionAttribute
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                var funcAttr = method.GetCustomAttribute<DecoFunctionAttribute>();
                if (funcAttr == null) continue;

                RegisterFunction(context, globalScope, method, funcAttr);
            }
        }
    }

    private static void RegisterFunction(CompilationContext context, Scope globalScope, MethodInfo method, DecoFunctionAttribute attr) {
        // Resolve Return Type
        var returnType = TypeUtils.ParseType(attr.ReturnType);

        // Resolve Parameters
        var parameters = method.GetParameters();

        // Validation: First parameter must be LibraryContext
        if (parameters.Length == 0 || parameters[0].ParameterType != typeof(LibraryContext)) {
            Console.WriteLine($"[Warning] Library function '{attr.Name}' ignored. The first parameter must be of type LibraryContext.");
            return;
        }

        var paramSymbols = new List<Symbol>();
        var paramTypes = new List<IType>();

        // Iterate parameters, skipping the first (LibraryContext)
        foreach (var param in parameters.Skip(1)) {
            var argAttr = param.GetCustomAttribute<DecoArgumentAttribute>();

            if (argAttr == null) {
                context.ErrorReporter.Report(new LibraryFunctionParameterError(
                    method.Name,
                    param.Name ?? "unknown",
                    method.DeclaringType?.Name ?? "unknown"
                ));
                continue;
            }

            var type = TypeUtils.ParseType(argAttr.Type);
            paramTypes.Add(type);
            paramSymbols.Add(new Symbol(
                param.Name ?? "arg",
                context.VariableCodeGen.Next(),
                type,
                SymbolKind.Parameter
            ));
        }

        // Return Symbol (used for type checking and potentially as a destination variable placeholder)
        var returnSymbol = new Symbol(
            $"{attr.Name}#return",
            context.VariableCodeGen.Next(),
            returnType,
            SymbolKind.Variable
        );

        // Construct Function Type
        var functionType = new FunctionType(returnType, paramTypes);

        // Create the LibraryFunctionSymbol
        // "###INTERNAL" indicates this doesn't have a physical .mcfunction file path
        var symbol = new LibraryFunctionSymbol(
            attr.Name,
            "###INTERNAL",
            functionType,
            paramSymbols,
            returnSymbol,
            method
        );

        // Create a dedicated scope for the function to hold its return symbol
        // (This helps with type resolution consistency)
        var funcScope = globalScope.CreateChild($"function {attr.Name}");
        funcScope.AddSymbol(returnSymbol);
        foreach (var p in paramSymbols) {
            funcScope.AddSymbol(p);
        }

        // Register to Global Scope
        globalScope.AddSymbol(symbol);
    }
}