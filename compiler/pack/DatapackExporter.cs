namespace Deco.Compiler.Pack;

/// <summary>
/// Exports a Datapack to a specified directory.
/// </summary>
public static class DatapackExporter {
    /// <summary>
    /// Exports the datapack to the specified output path.
    /// </summary>
    public static void Export(Datapack datapack, string outputPath) {
        Directory.CreateDirectory(outputPath);

        // Write pack.mcmeta
        File.WriteAllText(
            Path.Combine(outputPath, "pack.mcmeta"),
            datapack.PackMcmeta.ToString()
        );

        string dataPath = Path.Combine(outputPath, "data");
        Directory.CreateDirectory(dataPath);

        string namespacePath = Path.Combine(dataPath, datapack.Namespace);
        Directory.CreateDirectory(namespacePath);

        // Write functions
        if (datapack.Functions.Any()) {
            string functionsPath = Path.Combine(namespacePath, "function");
            Directory.CreateDirectory(functionsPath);

            foreach (var function in datapack.Functions) {
                string functionPath = Path.Combine(functionsPath, function.Location.Path + ".mcfunction");
                // Create subdirectories if needed
                Directory.CreateDirectory(Path.GetDirectoryName(functionPath)!);
                File.WriteAllLines(functionPath, function.Commands);
            }
        }


        // TODO: Export other resources like advancements, loot tables, etc.
    }
}
