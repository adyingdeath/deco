

using Antlr4.Runtime;
using Deco.Compiler;
using Deco.Compiler.Data;
using System.Text;

/// <summary>
/// 将字符串集合的每一行写入指定文件。
/// 如果文件不存在则创建，如果文件存在则根据 isAppend 参数决定是覆盖还是追加。
/// 并且会自动创建缺失的文件夹。
/// </summary>
/// <param name="filePath">要写入的文件完整路径。</param>
/// <param name="contents">要写入的字符串集合，每个字符串代表一行。</param>
/// <param name="isAppend">如果为 true，则将内容追加到文件末尾；如果为 false，则覆盖文件内容。默认为 false。</param>
void WriteFile(string filePath, IEnumerable<string> contents, bool isAppend = false)
{
    if (string.IsNullOrWhiteSpace(filePath))
    {
        throw new ArgumentException("文件路径不能为空或只包含空格。", nameof(filePath));
    }
    if (contents == null)
    {
        throw new ArgumentNullException(nameof(contents), "要写入的内容集合不能为 null。");
    }

    try
    {
        // 1. 获取文件所在的目录路径
        string? directoryPath = Path.GetDirectoryName(filePath);

        // 2. 如果目录路径不为空且目录不存在，则创建目录
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Console.WriteLine($"已创建目录: {directoryPath}");
        }

        // 3. 根据 isAppend 参数选择写入模式
        if (isAppend)
        {
            File.AppendAllLines(filePath, contents);
            Console.WriteLine($"内容已成功追加到文件: {filePath}");
        }
        else
        {
            File.WriteAllLines(filePath, contents);
            Console.WriteLine($"文件已成功写入/覆盖: {filePath}");
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"错误: 没有权限访问文件或目录。请检查文件权限。详情: {ex.Message}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"错误: 写入文件时发生IO错误。可能文件正在被其他进程使用。详情: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"发生未知错误: {ex.Message}");
    }
}

// --- Stage 1: Source Code Input ---
string sourceCode = @"
@load
@name(""deco:test/great"")
tick main_tick() {
    @`scoreboard players add @p ticks 1`;
    @`say running tick...`;
}

@tick
load main_load() {
    @`say tick!`;
}
";

Console.WriteLine("--- Source Code ---");
Console.WriteLine(sourceCode);
Console.WriteLine("-------------------");

// --- Stage 2: Parsing the Code ---
Console.WriteLine("\n--- Parsing Stage ---");
ICharStream stream = CharStreams.fromString(sourceCode);
DecoLexer lexer = new DecoLexer(stream);
CommonTokenStream tokens = new CommonTokenStream(lexer);
DecoParser parser = new DecoParser(tokens);
var tree = parser.program();
Console.WriteLine("Parsing complete.");

// --- Stage 3: Visiting the Parse Tree to Populate Data Model ---
Console.WriteLine("\n--- Visitor Stage ---");
var dataPack = new DataPack("generated_datapack");
var visitor = new DecoCodeVisitor(dataPack);
visitor.Visit(tree);
Console.WriteLine($"Visitor finished. Found {dataPack.Functions.Count} functions and {dataPack.Tags.Count} tags.");

// --- Stage 4: Generating .mcfunction Files ---
Console.WriteLine("\n--- Function Generation Stage ---");
string rootPath = dataPack.Name;
if (Directory.Exists(rootPath))
{
    Directory.Delete(rootPath, true);
}
Directory.CreateDirectory(rootPath);

foreach (var function in dataPack.Functions)
{
    string functionDirectory = Path.Combine(rootPath, "data", function.Location.Namespace, "function");
    Directory.CreateDirectory(functionDirectory);
    string filePath = Path.Combine(functionDirectory, $"{function.Location.Path}.mcfunction");

    FileUtil.WriteFile(filePath, function.Commands);
    Console.WriteLine($"  -> Generated function: {Path.GetFullPath(filePath)}");
}

// --- Stage 5: Generating Generic Tag Files ---
Console.WriteLine("\n--- Tag Generation Stage ---");
foreach (var tag in dataPack.Tags)
{
    // Determine the directory based on the tag type (e.g., "functions", "blocks")
    string tagTypeDirectoryName = tag.Type switch
    {
        TagType.Function => "function",
        TagType.Items => "items",
        TagType.Blocks => "blocks",
        TagType.EntityTypes => "entity_types",
        TagType.GameEvents => "game_events",
        TagType.Biomes => "biomes",
        // 默认情况：可以抛出异常，或者返回一个默认值（例如枚举名称的小写）
        _ => tag.Type.ToString().ToLowerInvariant()
        // 或者：_ => selectedTag.ToString().ToLowerInvariant()
    };

    string tagDirectory = Path.Combine(rootPath, "data", tag.Location.Namespace, "tags", tagTypeDirectoryName);
    Directory.CreateDirectory(tagDirectory);
    string filePath = Path.Combine(tagDirectory, $"{tag.Location.Path}.json");

    // Build the JSON content
    var jsonBuilder = new StringBuilder();
    jsonBuilder.AppendLine("{");
    jsonBuilder.AppendLine("  \"values\": [");
    jsonBuilder.Append(string.Join(",\n", tag.Values.Select(v => $"    \"{v}\"")));
    jsonBuilder.AppendLine();
    jsonBuilder.AppendLine("  ]");
    jsonBuilder.AppendLine("}");

    FileUtil.WriteFile(filePath, jsonBuilder.ToString());
    Console.WriteLine($"  -> Generated tag ({tag.Type}): {Path.GetFullPath(filePath)}");
}


Console.WriteLine("\n--- Compilation Finished ---");