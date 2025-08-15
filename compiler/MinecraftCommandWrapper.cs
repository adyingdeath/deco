using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A pre-processor class designed to transform code containing raw Minecraft commands.
/// The primary purpose of this class is to simplify a subsequent parsing stage, for example, using a tool like ANTLR.
/// 
/// Minecraft command syntax can be complex and difficult to define within a standard lexer grammar,
/// especially with its handling of NBT data, JSON strings, and various argument types.
/// 
/// This class isolates each Minecraft command by wrapping it in a unique delimiter (""").
/// By doing this, the ANTLR lexer can be configured with a very simple rule to recognize the entire
/// wrapped command as a single token (e.g., MINECRAFT_COMMAND). This offloads the complex task of
/// parsing the command's internal structure from the lexer to the parser, or even to a separate
/// dedicated parser, greatly simplifying the main grammar.
/// </summary>
public class MinecraftCommandWrapper
{
    /// <summary>
    /// A set of Minecraft command keywords. Using a HashSet provides efficient O(1) average time complexity for lookups.
    /// StringComparer.OrdinalIgnoreCase is used for case-insensitive matching.
    /// </summary>
    private readonly HashSet<string> _commandKeywords;

    public MinecraftCommandWrapper()
    {
        // Initialize with a list of common Minecraft command keywords.
        // You can easily add or remove keywords from this list as needed.
        _commandKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "advancement", "attribute", "ban", "ban-ip", "banlist", "bossbar",
            "clear", "clone", "damage", "data", "datapack", "debug", "defaultgamemode",
            "deop", "dialog", "difficulty", "effect", "enchant", "execute",
            "experience", "fill", "fillbiome", "forceload", "function", "gamemode",
            "gamerule", "give", "help", "item", "jfr", "kick", "kill", "list",
            "locate", "loot", "me", "msg", "op", "pardon", "pardon-ip", "particle",
            "perf", "place", "playsound", "publish", "random", "recipe", "reload",
            "ride", "rotate", "save-all", "save-off", "save-on", "say", "schedule",
            "scoreboard", "seed", "setblock", "setidletimeout", "setworldspawn",
            "spawnpoint", "spectate", "spreadplayers", "stop", "stopsound", "summon",
            "tag", "team", "teammsg", "teleport", "tell", "tellraw", "test", "tick",
            "time", "title", "tm", "tp", "transfer", "trigger", "version", "w",
            "waypoint", "weather", "whitelist", "worldborder", "xp"
        };//"return" this is special case and we should handle it specially.
    }

    /// <summary>
    /// Transforms a string of code by wrapping identified Minecraft commands with triple quotes (""").
    /// </summary>
    /// <param name="code">The raw source code string.</param>
    /// <returns>A new string with Minecraft commands wrapped.</returns>
    public string Transform(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return string.Empty;
        }

        var resultBuilder = new StringBuilder();
        int currentIndex = 0;

        while (currentIndex < code.Length)
        {
            // Find the start of the next potential command.
            int commandStartIndex = FindNextPotentialCommandStart(code, currentIndex);

            if (commandStartIndex == -1)
            {
                // No more potential commands found, append the rest of the code.
                resultBuilder.Append(code.Substring(currentIndex));
                break;
            }

            // Append the code segment before the command.
            resultBuilder.Append(code.Substring(currentIndex, commandStartIndex - currentIndex));
            
            // From the potential start, try to find the actual end of the command.
            int commandEndIndex = FindCommandEnd(code, commandStartIndex);

            if (commandEndIndex != -1)
            {
                // A complete command was found.
                string command = code.Substring(commandStartIndex, commandEndIndex - commandStartIndex + 1);
                command = command.TrimEnd(';').Replace("`", "\\`");
                resultBuilder.Append("@`").Append(command).Append("`;");
                currentIndex = commandEndIndex + 1;
            }
            else
            {
                // It looked like a command, but no valid end was found. Treat it as regular code.
                resultBuilder.Append(code[commandStartIndex]);
                currentIndex = commandStartIndex + 1;
            }
        }

        return resultBuilder.ToString();
    }
    
    /// <summary>
    /// Scans from a given start index to find the beginning of the next word that is a command keyword.
    /// </summary>
    private int FindNextPotentialCommandStart(string code, int startIndex)
    {
        for (int i = startIndex; i < code.Length; i++)
        {
            // Skip whitespace.
            if (char.IsWhiteSpace(code[i]))
            {
                continue;
            }

            // Check if the current position matches the start of any command keyword.
            foreach (var keyword in _commandKeywords)
            {
                if (i + keyword.Length <= code.Length && 
                    code.Substring(i, keyword.Length).Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    // Ensure it's a whole word by checking the character that follows.
                    // It must be the end of the string, whitespace, or a semicolon.
                    if (i + keyword.Length == code.Length || 
                        char.IsWhiteSpace(code[i + keyword.Length]) || 
                        code[i + keyword.Length] == ';')
                    {
                        return i;
                    }
                }
            }
        }
        return -1; // Not found.
    }

    /// <summary>
    /// Finds the terminating semicolon (';') of a command, starting from its initial index.
    /// This method correctly handles semicolons that appear inside strings ("...") and NBT arrays ([...]),
    /// preventing them from being misinterpreted as the end of the command.
    /// </summary>
    /// <param name="code">The full code string.</param>
    /// <param name="startIndex">The starting index of the command.</param>
    /// <returns>The index of the command's closing semicolon, or -1 if not found.</returns>
    private int FindCommandEnd(string code, int startIndex)
    {
        bool inString = false;
        int squareBracketDepth = 0; // Tracks nesting level of NBT arrays like [I;...].

        for (int i = startIndex; i < code.Length; i++)
        {
            char currentChar = code[i];
            char previousChar = (i > 0) ? code[i - 1] : '\0';

            // 1. Handle string literals ("...").
            // Ignores escaped quotes (\").
            if (currentChar == '"' && previousChar != '\\')
            {
                inString = !inString;
                continue;
            }

            // If inside a string, ignore all other special characters.
            if (inString)
            {
                continue;
            }

            // 2. Handle NBT arrays ([...]).
            if (currentChar == '[')
            {
                squareBracketDepth++;
            }
            else if (currentChar == ']')
            {
                if (squareBracketDepth > 0)
                {
                    squareBracketDepth--;
                }
            }
            
            // 3. Check for the end of the command.
            // The condition is: the character is a semicolon, AND we are not inside a string,
            // AND we are not inside an NBT array.
            if (currentChar == ';' && !inString && squareBracketDepth == 0)
            {
                return i; // This is the true end of the command.
            }
        }

        return -1; // No valid command ending was found.
    }
}