using System;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents a Minecraft resource location, like 'minecraft:stone' or 'deco:my_function'.
    /// This class is immutable.
    /// </summary>
    public class ResourceLocation
    {
        public string Namespace { get; }
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLocation"/> class with a specified path and namespace.
        /// </summary>
        /// <param name="path">The path part of the resource location.</param>
        /// <param name="namespace">The namespace part of the resource location.</param>
        /// <exception cref="ArgumentException">Thrown if the path or namespace is null or empty.</exception>
        public ResourceLocation(string path, string @namespace)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Namespace cannot be null or empty.", nameof(@namespace));

            Namespace = @namespace;
            Path = path;
        }

        /// <summary>
        /// Parses a string into a <see cref="ResourceLocation"/>.
        /// </summary>
        /// <param name="location">The string representation of the resource location (e.g., 'minecraft:stone' or 'stone').</param>
        /// <param name="defaultNamespace">The namespace to use if the location string does not specify one.</param>
        /// <returns>A new <see cref="ResourceLocation"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the location string is null or empty.</exception>
        /// <exception cref="FormatException">Thrown if the location string is malformed.</exception>
        public static ResourceLocation Parse(string location, string defaultNamespace)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location string cannot be null or empty.", nameof(location));
            }

            if (location.Contains(':'))
            {
                var parts = location.Split(':', 2);
                if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new FormatException($"Invalid resource location format: '{location}'. Expected 'namespace:path'.");
                }
                return new ResourceLocation(parts[1], parts[0]);
            }
            else
            {
                return new ResourceLocation(location, defaultNamespace);
            }
        }

        public override string ToString() => $"{Namespace}:{Path}";
    }
}
