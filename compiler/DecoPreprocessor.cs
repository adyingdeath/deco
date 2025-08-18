using System;
using System.Collections.Generic;
using System.Text;

namespace Deco.Compiler {
    /// <summary>
    /// A pre-processor class designed to transform deco code before parsing.
    /// It performs two main tasks:
    /// 1. Strips all line and block comments.
    /// 2. Wraps bare Minecraft commands in special delimiters to simplify parsing.
    /// </summary>
    public class DecoPreprocessor {
        /// <summary>
        /// A set of Minecraft command keywords. Using a HashSet provides efficient O(1) average time complexity for lookups.
        /// StringComparer.OrdinalIgnoreCase is used for case-insensitive matching.
        /// </summary>
        private readonly HashSet<string> _commandKeywords;

        public DecoPreprocessor() {
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
            };//"return" this is a special case and we should handle it specially.
        }

        /// <summary>
        /// Preprocesses the source code by stripping comments and wrapping Minecraft commands.
        /// </summary>
        /// <param name="code">The raw source code string.</param>
        /// <returns>A new string with comments removed and commands wrapped.</returns>
        public string Preprocess(string code) {
            if (string.IsNullOrEmpty(code)) {
                return string.Empty;
            }

            var withoutComments = StripComments(code);
            return WrapMinecraftCommands(withoutComments);
        }

        private string StripComments(string code) {
            if (string.IsNullOrEmpty(code)) {
                return string.Empty;
            }

            var resultBuilder = new StringBuilder();
            bool inString = false;
            char stringChar = '\0'; // Can be ' or "

            for (int i = 0; i < code.Length; i++) {
                char currentChar = code[i];
                char previousChar = (i > 0) ? code[i - 1] : '\0';

                if (!inString && (currentChar == '"' || currentChar == '\'')) {
                    inString = true;
                    stringChar = currentChar;
                } else if (inString && currentChar == stringChar && previousChar != '\\') {
                    inString = false;
                }

                if (!inString) {
                    // Check for line comment
                    if (currentChar == '/' && i + 1 < code.Length && code[i + 1] == '/') {
                        int j = i + 2;
                        while (j < code.Length && code[j] != '\n' && code[j] != '\r') {
                            j++;
                        }
                        i = j - 1;
                        continue;
                    }

                    // Check for block comment
                    if (currentChar == '/' && i + 1 < code.Length && code[i + 1] == '*') {
                        int j = i + 2;
                        while (j + 1 < code.Length && !(code[j] == '*' && code[j + 1] == '/')) {
                            j++;
                        }
                        i = j + 1;
                        continue;
                    }
                }

                resultBuilder.Append(currentChar);
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// Transforms a string of code by wrapping identified Minecraft commands with `@`...`;`.
        /// This method correctly ignores commands inside any string literal (e.g., "...", c`...`, or already-wrapped @`...`).
        /// </summary>
        /// <param name="code">The source code string, with comments already stripped.</param>
        /// <returns>A new string with bare Minecraft commands wrapped.</returns>
        private string WrapMinecraftCommands(string code) {
            if (string.IsNullOrEmpty(code)) {
                return string.Empty;
            }

            var resultBuilder = new StringBuilder();
            for (int i = 0; i < code.Length; i++) {
                char currentChar = code[i];

                // 1. Skip over double-quoted strings
                if (currentChar == '"') {
                    int endIndex = FindEndOfString(code, i, '"');
                    if (endIndex != -1) {
                        resultBuilder.Append(code.Substring(i, endIndex - i + 1));
                        i = endIndex;
                        continue;
                    }
                }

                // 2. Skip over any tagged backtick strings (like c`...` or our own @`...`)
                if (i + 1 < code.Length && code[i + 1] == '`') {
                    // The character before a backtick must not be a whitespace to be a tag.
                    int endIndex = FindEndOfBacktickString(code, i + 1);
                    if (endIndex != -1) {
                        resultBuilder.Append(code.Substring(i, endIndex - i + 1));
                        i = endIndex;
                        continue;
                    }
                }

                // 3. Check for a potential bare Minecraft command
                if (IsBareCommandAt(code, i, out string _)) {
                    int commandEndIndex = FindCommandEnd(code, i);
                    if (commandEndIndex != -1) {
                        string command = code.Substring(i, commandEndIndex - i + 1);
                        // Trim the trailing semicolon for wrapping, then re-add it.
                        command = command.TrimEnd(';').Replace("`", "\\`");
                        resultBuilder.Append("@`").Append(command).Append("`;");
                        i = commandEndIndex;
                        continue;
                    }
                }

                // 4. If none of the above, it's a regular character.
                resultBuilder.Append(currentChar);
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// Finds the end of a double-quoted or single-quoted string, handling escaped characters.
        /// </summary>
        private int FindEndOfString(string code, int startIndex, char quoteChar) {
            for (int i = startIndex + 1; i < code.Length; i++) {
                if (code[i] == quoteChar && code[i - 1] != '\\') {
                    return i;
                }
            }
            return -1; // Not found
        }

        /// <summary>
        /// Finds the end of a backtick-enclosed string, handling escaped backticks.
        /// </summary>
        private int FindEndOfBacktickString(string code, int startBacktickIndex) {
            for (int i = startBacktickIndex + 1; i < code.Length; i++) {
                if (code[i] == '`' && code[i - 1] != '\\') {
                    return i;
                }
            }
            return -1; // Not found
        }

        /// <summary>
        /// Checks if the character at the given index is the start of a bare Minecraft command.
        /// A command must be a whole word and not be part of another identifier (e.g., `myObject.say` should not match).
        /// </summary>
        private bool IsBareCommandAt(string code, int index, out string matchedKeyword) {
            matchedKeyword = null;

            // A command must be preceded by whitespace, a delimiter, or be at the start of the code.
            if (index > 0) {
                char prevChar = code[index - 1];
                if (!char.IsWhiteSpace(prevChar) && prevChar != '{' && prevChar != '}' && prevChar != '(' && prevChar != ')' && prevChar != ';') {
                    return false;
                }
            }

            foreach (var keyword in _commandKeywords) {
                if (index + keyword.Length <= code.Length &&
                    string.Compare(code, index, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                    // Ensure it's a whole word by checking the character that follows.
                    if (index + keyword.Length == code.Length ||
                        char.IsWhiteSpace(code[index + keyword.Length]) ||
                        code[index + keyword.Length] == ';') {
                        matchedKeyword = keyword;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the terminating semicolon (';') of a command, starting from its initial index.
        /// This method correctly handles semicolons that appear inside strings ("...") and NBT arrays ([...]),
        /// preventing them from being misinterpreted as the end of the command.
        /// </summary>
        /// <param name="code">The full code string.</param>
        /// <param name="startIndex">The starting index of the command.</param>
        /// <returns>The index of the command's closing semicolon, or -1 if not found.</returns>
        private int FindCommandEnd(string code, int startIndex) {
            bool inString = false;
            int squareBracketDepth = 0; // Tracks nesting level of NBT arrays like [I;...].

            for (int i = startIndex; i < code.Length; i++) {
                char currentChar = code[i];
                char previousChar = (i > 0) ? code[i - 1] : '\0';

                // 1. Handle string literals ("...").
                // Ignores escaped quotes (\").
                if (currentChar == '"' && previousChar != '\\') {
                    inString = !inString;
                    continue;
                }

                // If inside a string, ignore all other special characters.
                if (inString) {
                    continue;
                }

                // 2. Handle NBT arrays ([...]).
                if (currentChar == '[') {
                    squareBracketDepth++;
                } else if (currentChar == ']') {
                    if (squareBracketDepth > 0) {
                        squareBracketDepth--;
                    }
                }

                // 3. Check for the end of the command.
                // The condition is: the character is a semicolon, AND we are not inside a string,
                // AND we are not inside an NBT array.
                if (currentChar == ';' && !inString && squareBracketDepth == 0) {
                    return i; // This is the true end of the command.
                }
            }

            return -1; // No valid command ending was found.
        }
    }
}