namespace Deco.Compiler.Types;

/// <summary>
/// Abstract base class for all types in the Deco language.
/// </summary>
public abstract class IType(string name) {
    public string Name { get; } = name;
    public abstract bool Equals(IType type);

    /// <summary>
    /// Indicates if this type represents a numeric value (int, float).
    /// </summary>
    public abstract bool IsNumeric { get; }

    /// <summary>
    /// Indicates if this type can be stored in a Minecraft scoreboard (int, bool).
    /// </summary>
    public abstract bool IsStorableInScoreboard { get; }

    /// <summary>
    /// Gets the default value for this type as a string literal.
    /// This will be used in Datapack to initialize those unassigned variables.
    /// </summary>
    public abstract string GetDefaultValueAsString();

    /// <summary>
    /// Checks if this type can be assigned to a variable of the target type.
    /// Allows for implicit conversions.
    /// </summary>
    public virtual bool IsAssignableTo(IType targetType) {
        // Default rule: types must be exactly equal.
        return Equals(targetType);
    }
}

/// <summary>
/// Represents primitive types like int, bool, string, void.
/// </summary>
public class PrimitiveType(string name) : IType(name) {
    public override string ToString() => Name;
    public override bool Equals(IType type) {
        return type is PrimitiveType primitiveType && Name.Equals(primitiveType.Name);
    }

    public override bool IsNumeric => Name is "int" or "float";
    public override bool IsStorableInScoreboard => Name is "int" or "bool";

    public override string GetDefaultValueAsString() {
        return Name switch {
            "int" or "bool" => "0",
            "float" => "0.0f",
            "string" => "\"\"", // Empty string literal
            _ => ""
        };
    }

    public override bool IsAssignableTo(IType targetType) {
        if (base.IsAssignableTo(targetType)) {
            return true;
        }

        // Implicit conversion rule: int can be assigned to float
        if (this.Equals(TypeUtils.IntType) && targetType.Equals(TypeUtils.FloatType)) {
            return true;
        }

        // Add more rules here if needed in the future

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
        if (type is not FunctionType functionType) return false;
        if (!ReturnType.Equals(functionType.ReturnType)) return false;
        if (ParameterTypes.Count != functionType.ParameterTypes.Count) return false;

        for (int i = 0; i < ParameterTypes.Count; i++) {
            if (!ParameterTypes[i].Equals(functionType.ParameterTypes[i]))
                return false;
        }
        return true;
    }

    public override bool IsNumeric => false;
    public override bool IsStorableInScoreboard => false;
    public override string GetDefaultValueAsString() => ""; // Functions have no literal default value
}

public class UnresolvedType(string name) : IType(name) {
    public override string ToString() => Name;
    public override bool Equals(IType type) {
        return type switch {
            UnresolvedType unresolved => Name == unresolved.Name,
            PrimitiveType primitive => Name == primitive.Name,
            _ => false
        };
    }

    public override bool IsNumeric => false; // Cannot determine until resolved
    public override bool IsStorableInScoreboard => false; // Cannot determine until resolved
    public override string GetDefaultValueAsString() => ""; // Cannot determine until resolved
}

/// <summary>
/// Utility class for type operations and predefined types, containing shared
/// type instances and parsing logic.
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
    /// </summary>
    public static IType ParseType(string typeName) {
        return typeName switch {
            "int" => IntType,
            "float" => FloatType,
            "bool" => BoolType,
            "string" => StringType,
            "void" => VoidType,
            _ => new UnresolvedType(typeName)
        };
    }
}