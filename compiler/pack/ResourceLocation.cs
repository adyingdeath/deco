namespace Deco.Compiler.Pack;

/// <summary>
/// Represents a Minecraft resource location, consisting of a namespace and a path.
/// </summary>
public class ResourceLocation(string @namespace, string path) {
    /// <summary>
    /// The namespace of the resource.
    /// </summary>
    public string Namespace { get; } = @namespace;

    /// <summary>
    /// The path of the resource.
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    /// Returns the string representation in the format "namespace:path".
    /// </summary>
    public override string ToString() => $"{Namespace}:{Path}";

    /// <summary>
    /// Parses a resource location string into a ResourceLocation object.
    /// </summary>
    public static ResourceLocation Parse(string resourceLocation) {
        var parts = resourceLocation.Split(':', 2);
        return parts.Length == 2
            ? new ResourceLocation(parts[0], parts[1])
            : new ResourceLocation("minecraft", parts[0]);
    }

    public override bool Equals(object? obj) {
        return obj is ResourceLocation res
            && res.Namespace.Equals(Namespace)
            && res.Path.Equals(Path);
    }

    public override int GetHashCode() {
        /* Here we directly use the string's GetHashCode because two Resource
        Location objects are supposed to be the same if they have the same string
        form. */
        return this.ToString().GetHashCode();
    }
}

interface IHasResourceLocation<T> {
    public ResourceLocation Location { get; set; }
    public T SetLocation(ResourceLocation location);
}

public abstract class HasResourceLocationBase<T> : IHasResourceLocation<T> where T : HasResourceLocationBase<T> {
    public ResourceLocation Location { get; set; } = new("minecraft", "deco");
    public T SetLocation(ResourceLocation location) {
        Location = location;
        return (T)this;
    }
}
