namespace Deco.Types;

/// <summary>
/// Abstract base class for all types in the Deco language.
/// </summary>
public abstract class Type(string name) {
    public string Name { get; } = name;
    public abstract bool Equals(Type type);
}

/// <summary>
/// Represents primitive types like int, bool, string, void.
/// </summary>
public class PrimitiveType(string name) : Type(name) {
    public override string ToString() => Name;
    public override bool Equals(Type type) {
        if (type is PrimitiveType primitiveType) {
            return Name.Equals(primitiveType.Name);
        }
        return false;
    }
}

/// <summary>
/// Represents function types with return type and parameter types.
/// </summary>
public class FunctionType(Type returnType, List<Type> parameterTypes) : Type("function") {
    public Type ReturnType { get; } = returnType;
    public List<Type> ParameterTypes { get; } = parameterTypes ?? [];

    public override string ToString() => $"{ReturnType}({string.Join(", ", ParameterTypes)})";
    public override bool Equals(Type type) {
        if (type is FunctionType functionType) {
            if (!ReturnType.Equals(functionType.ReturnType))
                return false;
            if (parameterTypes.Count != functionType.ParameterTypes.Count)
                return false;
            for (int i = 0; i < ParameterTypes.Count; i++) {
                if (!ParameterTypes[i].Equals(functionType.ParameterTypes[i]))
                    return false;
            }
            return true;
        }
        return false;
    }
}

public class UnresolvedType(string name) : Type(name) {
    public override string ToString() => Name;
    public override bool Equals(Type type) {
        return Name.Equals(type.Name);
    }
}

/// <summary>
/// Utility class for type operations and predefined types.
/// </summary>
public static class TypeUtils {
    public static readonly PrimitiveType IntType = new("int");
    public static readonly PrimitiveType BoolType = new("bool");
    public static readonly PrimitiveType StringType = new("string");
    public static readonly PrimitiveType VoidType = new("void");

    /// <summary>
    /// Parses a type from a string name. For primitives, returns the predefined instance.
    /// For unknown types, creates a new PrimitiveType.
    /// </summary>
    public static Type ParseType(string typeName) {
        return typeName switch {
            "int" => IntType,
            "bool" => BoolType,
            "string" => StringType,
            "void" => VoidType,
            _ => new UnresolvedType(typeName)
        };
    }
}
