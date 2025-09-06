using Deco.Compiler.Data;

namespace Deco.Compiler.Library.Types {
    public class CoreTypeSingleton {
        public static VoidType Void { get; } = new VoidType();
        public static IntType Int { get; } = new IntType();
        public static BoolType Bool { get; } = new BoolType();
        public static StringType String { get; } = new StringType();
        public static FloatType Float { get; } = new FloatType();
        public static FunctionType Function { get; } = new FunctionType();
    }

    /// <summary>
    /// Built-in void type for functions with no return value
    /// </summary>
    public class VoidType : IDecoType {
        public string Package => "deco.core";
        public string Name => "void";

        public void Assign(LibContext context, Variable target, Variable source) {
            // Void assignments don't make sense, but this might be called in edge cases
            throw new System.NotSupportedException("Cannot assign void values.");
        }

        public void Initialize(LibContext context, Variable target) {
            // No initialization needed for void
        }
    }

    /// <summary>
    /// Built-in integer type
    /// </summary>
    public class IntType : IDecoType {
        public string Package => "deco.core";
        public string Name => "int";

        public void Assign(LibContext context, Variable target, Variable source) {
            // [TODO] Handle Const<type> for statements like `int a = 1` or `int a = 1 + 1`
            context.CurrentMcFunction.Commands.Add($"scoreboard players operation {target.StorageName} {context.DataPack.ID} = {source.StorageName} {context.DataPack.ID}");
        }

        public void Initialize(LibContext context, Variable target) {
            context.CurrentMcFunction.Commands.Add($"scoreboard players set {target.StorageName} {context.DataPack.ID} 0");
        }
    }

    /// <summary>
    /// Built-in boolean type (stored as integer 0/1)
    /// </summary>
    public class BoolType : IDecoType {
        public string Package => "deco.core";
        public string Name => "bool";

        public void Assign(LibContext context, Variable target, Variable source) {
            context.CurrentMcFunction.Commands.Add($"scoreboard players operation {target.StorageName} {context.DataPack.ID} = {source.StorageName} {context.DataPack.ID}");
        }

        public void Initialize(LibContext context, Variable target) {
            context.CurrentMcFunction.Commands.Add($"scoreboard players set {target.StorageName} {context.DataPack.ID} 0");
        }
    }

    /// <summary>
    /// Built-in string type (stored in storage)
    /// </summary>
    public class StringType : IDecoType {
        public string Package => "deco.core";
        public string Name => "string";

        public void Assign(LibContext context, Variable target, Variable source) {
            context.CurrentMcFunction.Commands.Add($"data modify storage {context.DataPack.ID} {target.StorageName} set from storage {context.DataPack.ID} {source.StorageName}");
        }

        public void Initialize(LibContext context, Variable target) {
            context.CurrentMcFunction.Commands.Add($"data modify storage {context.DataPack.ID} {target.StorageName} set value \"\"");
        }
    }

    /// <summary>
    /// Built-in float type
    /// </summary>
    public class FloatType : IDecoType {
        public string Package => "deco.core";
        public string Name => "float";

        public void Assign(LibContext context, Variable target, Variable source) {
            context.CurrentMcFunction.Commands.Add($"scoreboard players operation {target.StorageName} {context.DataPack.ID} = {source.StorageName} {context.DataPack.ID}");
        }

        public void Initialize(LibContext context, Variable target) {
            context.CurrentMcFunction.Commands.Add($"scoreboard players set {target.StorageName} {context.DataPack.ID} 0.0f");
        }
    }

    /// <summary>
    /// Represents the type of a function symbol in the Deco language.
    /// Functions cannot be assigned to variables or initialized as regular variables.
    /// </summary>
    public class FunctionType : IDecoType {
        public string Package => "deco.core";
        public string Name => "function";

        public void Assign(LibContext context, Variable target, Variable source) {
            throw new System.NotSupportedException("Cannot assign function values.");
        }

        public void Initialize(LibContext context, Variable target) {
            // Functions are not initialized like variables
        }
    }
}
