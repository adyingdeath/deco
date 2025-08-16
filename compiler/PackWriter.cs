using Deco.Compiler.Data;
using System.Text;

namespace Deco.Compiler
{
    /// <summary>
    /// Handles writing the DataPack structure to the file system.
    /// This includes generating .mcfunction files for functions and .json files for tags.
    /// </summary>
    public class PackWriter
    {
        private readonly DataPack _dataPack;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackWriter"/> class.
        /// </summary>
        /// <param name="dataPack">The data pack model to be written to disk.</param>
        public PackWriter(DataPack dataPack)
        {
            _dataPack = dataPack;
        }

        /// <summary>
        /// Writes the entire data pack to the file system, including all functions and tags.
        /// It creates the necessary directory structure.
        /// </summary>
        public void Write()
        {
            // Get the root path for the datapack and ensure the directory is clean
            string rootPath = _dataPack.Name;
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }
            Directory.CreateDirectory(rootPath);

            Console.WriteLine("\n--- Function Generation Stage ---");
            WriteFunctions(rootPath);

            Console.WriteLine("\n--- Tag Generation Stage ---");
            WriteTags(rootPath);
        }

        /// <summary>
        /// Writes all the functions from the data pack to their corresponding .mcfunction files.
        /// </summary>
        /// <param name="rootPath">The root directory of the data pack.</param>
        private void WriteFunctions(string rootPath)
        {
            foreach (var function in _dataPack.Functions.Items)
            {
                string functionDirectory = Path.Combine(rootPath, "data", function.Location.Namespace, "function");
                Directory.CreateDirectory(functionDirectory); // Ensure directory exists
                string filePath = Path.Combine(functionDirectory, $"{function.Location.Path}.mcfunction");

                Util.WriteFile(filePath, function.Commands);
                Console.WriteLine($"  -> Generated function: {Path.GetFullPath(filePath)}");
            }
        }

        /// <summary>
        /// Writes all the tags from the data pack to their corresponding .json files.
        /// </summary>
        /// <param name="rootPath">The root directory of the data pack.</param>
        private void WriteTags(string rootPath)
        {
            foreach (var tag in _dataPack.Tags)
            {
                // Determine the directory name based on the tag type (e.g., "functions", "blocks")
                string tagTypeDirectoryName = tag.Type.ToString().ToLowerInvariant();

                string tagDirectory = Path.Combine(rootPath, "data", tag.Location.Namespace, "tags", tagTypeDirectoryName);
                Directory.CreateDirectory(tagDirectory); // Ensure directory exists
                string filePath = Path.Combine(tagDirectory, $"{tag.Location.Path}.json");

                // Build the JSON content
                var jsonBuilder = new StringBuilder();
                jsonBuilder.AppendLine("{");
                jsonBuilder.AppendLine("  \"values\": [");
                jsonBuilder.Append(string.Join(",\n", tag.Values.Select(v => $"    \"{v}\"")));
                jsonBuilder.AppendLine();
                jsonBuilder.AppendLine("  ]");
                jsonBuilder.AppendLine("}");

                Util.WriteFile(filePath, jsonBuilder.ToString());
                Console.WriteLine($"  -> Generated tag ({tag.Type}): {Path.GetFullPath(filePath)}");
            }
        }
    }
}