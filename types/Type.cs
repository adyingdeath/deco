namespace Deco.Types;

/// <summary>
/// Abstract base class for all types in the Deco language.
/// </summary>
public abstract class IType(string name) {
    public string Name { get; } = name;
    public abstract bool Equals(IType type);
}

/// <summary>
/// Represents primitive types like int, bool, string, void.
/// </summary>
public class PrimitiveType(string name) : IType(name) {
    public override string ToString() => Name;
    public override bool Equals(IType type) {
        if (type is PrimitiveType primitiveType) {
            return Name.Equals(primitiveType.Name);
        }
        return false;
    }
}

/// <summary>
/// Represents function types with return type and parameter types.
/// </summary>
public class FunctionType(IType returnType, List<IType> parameterTypes) : IType("function") {
    public IType ReturnType { get; } = returnType;
    public List<IType> ParameterTypes { get; } = parameterTypes ?? [];

    public override string ToString() => $"({string.Join(", ", ParameterTypes)}) => {ReturnType}";
    public override bool Equals(IType type) {
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

public class UnresolvedType(string name) : IType(name) {
    public override string ToString() => Name;
    public override bool Equals(IType type) {
        // Unresolved types can only equal other unresolved types with same name,
        // or primitive types with same name (for type resolution purposes)
        if (type is UnresolvedType unresolved) {
            return Name == unresolved.Name;
        }
        if (type is PrimitiveType primitive) {
            return Name == primitive.Name;
        }
        return false;
    }
}

/// <summary>
/// Utility class for type operations and predefined types.
/// </summary>
public static class TypeUtils {
    public static readonly PrimitiveType IntType = new("int");
    public static readonly PrimitiveType FloatType = new("float");
    public static readonly PrimitiveType BoolType = new("bool");
    public static readonly PrimitiveType StringType = new("string");
    public static readonly PrimitiveType VoidType = new("void");
    public static readonly UnresolvedType UnknownType = new("<unknown>");

    /// <summary>
    /// Parses a type from a string name. For primitives, returns the predefined instance.
    /// For unknown types, creates a new PrimitiveType.
    /// </summary>
    public static IType ParseType(string typeName) {
        return typeName switch {
            "int" => IntType,
            "bool" => BoolType,
            "string" => StringType,
            "void" => VoidType,
            _ => new UnresolvedType(typeName)
        };
    }

    public static bool IsNumeric(IType type) {
        return type.Equals(IntType) || type.Equals(FloatType);
    }

    /// <summary>
    /// This helper function is used to check if a given type should be stored
    /// in scoreboard or not
    /// </summary>
    /// <param name="type"></param>
    /// <returns>True if the type can be stored in scoreboard. False if it can
    /// only be stored in storage or something else.</returns>
    public static bool IsScoreboard(IType type) {
        return type.Equals(IntType) || type.Equals(BoolType);
    }

    public static string GetInitialValue(IType type) {
        if (type.Equals(IntType) || type.Equals(BoolType)) {
            return "0";
        } else if (type.Equals(FloatType)) {
            return "0.0f";
        } else if (type.Equals(StringType)) {
            return "";
        }
        return "";
    }

    /// <summary>
    /// Checks if a type is unresolved (either unknown or an unresolved type).
    /// </summary>
    public static bool IsUnresolved(IType type) {
        return type is UnresolvedType || type == UnknownType;
    }

    /// <summary>
    /// Gets the resolved type if possible, otherwise returns the original type.
    /// This is a safe way to attempt type resolution.
    /// </summary>
    public static IType GetResolvedType(IType type) {
        if (type is UnresolvedType unresolved) {
            return ParseType(unresolved.Name);
        }
        return type;
    }
}
