namespace Deco.Compiler.Lib;

public enum ArgumentType {
    CONSTANT,
    SCOREBOARD,
    STORAGE,
}

public class Argument(ArgumentType type, string location, string name) {
    public ArgumentType Type = type;
    /// <summary>
    /// The location of this argument.
    /// For scoreboard, it's the scoreboard's name, like "deco".
    /// For storage, it's the storage's resource location, like "minecraft:abc".
    /// </summary>
    public string Location = location;
    /// <summary>
    /// For scoreboard, it's the <playerName> where this argument is stored in.
    /// For storage, it's the path in the storage where this argument is stored in.
    /// For constant, it's the constant's value.
    /// </summary>
    public string Name = name;
}