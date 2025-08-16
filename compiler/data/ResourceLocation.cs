using System;

namespace Deco.Compiler.Data
{
    /// <summary>
    /// Represents a Minecraft resource location, like 'minecraft:stone' or 'deco:my_function'.
    /// </summary>
    public class ResourceLocation
    {
        public string Namespace { get; private set; }
        public string Path { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceLocation"/> class from a string.
        /// If no namespace is provided, 'minecraft' is used as the default.
        /// </summary>
        /// <param name="location">The string representation of the resource location (e.g., 'minecraft:stone' or 'stone').</param>
        public ResourceLocation(string location)
        {
            // Set a default namespace before parsing.
            this.Namespace = "minecraft";
            SetLocation(location);
        }

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
        /// Sets the resource location from a string. If the location string does not contain a namespace,
        /// the existing namespace on the object is preserved.
        /// </summary>
        /// <param name="location">The string representation of the resource location (e.g., 'minecraft:stone' or 'stone').</param>
        public void SetLocation(string location)
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
                this.Namespace = parts[0];
                this.Path = parts[1];
            }
            else
            {
                // If no namespace is provided in the string, only update the path,
                // preserving the existing namespace.
                this.Path = location;
            }
        }

        public override string ToString() => $"{Namespace}:{Path}";
    }
}
