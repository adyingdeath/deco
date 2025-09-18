namespace Deco;

/// <summary>
/// Provides utility methods for common file operations, including creating missing directories.
/// </summary>
public static class Util {
    /// <summary>
    /// Writes multiple lines of content to the specified file.
    /// Creates the file if it doesn't exist, and automatically creates any missing parent directories.
    /// If the file exists, its contents will be overwritten.
    /// </summary>
    /// <param name="filePath">The full path to the file to write to.</param>
    /// <param name="contents">An enumerable collection of strings, where each string represents a line to write.</param>
    public static void WriteFile(string filePath, IEnumerable<string> contents) {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        if (contents == null) {
            throw new ArgumentNullException(nameof(contents), "The content collection to write cannot be null.");
        }

        try {
            // 1. Get the directory path where the file should reside
            string? directoryPath = Path.GetDirectoryName(filePath);

            // 2. If the directory path is not empty and the directory doesn't exist, create it
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"[FileUtil] Directory created: {directoryPath}");
            }

            // 3. Write all lines to the file.
            //    If the file does not exist, it will be created.
            //    If the file exists, its contents will be overwritten.
            File.WriteAllLines(filePath, contents);
            Console.WriteLine($"[FileUtil] File successfully written/overwritten: {filePath}");
        }
        catch (UnauthorizedAccessException ex) {
            Console.WriteLine($"[FileUtil Error] Access to the path '{filePath}' is denied. Please check file permissions. Details: {ex.Message}");
        }
        catch (IOException ex) {
            Console.WriteLine($"[FileUtil Error] An I/O error occurred while writing to the file. It might be in use by another process. Details: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"[FileUtil Error] An unknown error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes a single string content to the specified file.
    /// Creates the file if it doesn't exist, and automatically creates any missing parent directories.
    /// If the file exists, its contents will be overwritten.
    /// This is an overload for writing a single line/text block.
    /// </summary>
    /// <param name="filePath">The full path to the file to write to.</param>
    /// <param name="content">The single string content to write to the file.</param>
    public static void WriteFile(string filePath, string content) {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        // It's acceptable for 'content' to be null if we want to write an empty file or clear it.
        // However, if strict non-null content is required, uncomment the following:
        // if (content == null)
        // {
        //     throw new ArgumentNullException(nameof(content), "The content string to write cannot be null.");
        // }

        try {
            // 1. Get the directory path where the file should reside
            string? directoryPath = Path.GetDirectoryName(filePath);

            // 2. If the directory path is not empty and the directory doesn't exist, create it
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"[FileUtil] Directory created: {directoryPath}");
            }

            // 3. Write all text to the file.
            //    If the file does not exist, it will be created.
            //    If the file exists, its contents will be overwritten.
            File.WriteAllText(filePath, content); // Using File.WriteAllText for single string
            Console.WriteLine($"[FileUtil] File successfully written/overwritten: {filePath}");
        }
        catch (UnauthorizedAccessException ex) {
            Console.WriteLine($"[FileUtil Error] Access to the path '{filePath}' is denied. Please check file permissions. Details: {ex.Message}");
        }
        catch (IOException ex) {
            Console.WriteLine($"[FileUtil Error] An I/O error occurred while writing to the file. It might be in use by another process. Details: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"[FileUtil Error] An unknown error occurred: {ex.Message}");
        }
    }

    // You might also want to add similar methods for appending content, e.g.:
    /// <summary>
    /// Appends multiple lines of content to the specified file.
    /// Creates the file if it doesn't exist, and automatically creates any missing parent directories.
    /// </summary>
    /// <param name="filePath">The full path to the file to append to.</param>
    /// <param name="contents">An enumerable collection of strings, where each string represents a line to append.</param>
    public static void AppendFile(string filePath, IEnumerable<string> contents) {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }
        if (contents == null) {
            throw new ArgumentNullException(nameof(contents), "The content collection to append cannot be null.");
        }

        try {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"[FileUtil] Directory created: {directoryPath}");
            }

            File.AppendAllLines(filePath, contents);
            Console.WriteLine($"[FileUtil] Content successfully appended to file: {filePath}");
        }
        catch (UnauthorizedAccessException ex) {
            Console.WriteLine($"[FileUtil Error] Access to the path '{filePath}' is denied. Please check file permissions. Details: {ex.Message}");
        }
        catch (IOException ex) {
            Console.WriteLine($"[FileUtil Error] An I/O error occurred while appending to the file. It might be in use by another process. Details: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"[FileUtil Error] An unknown error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Appends a single string content to the specified file.
    /// Creates the file if it doesn't exist, and automatically creates any missing parent directories.
    /// </summary>
    /// <param name="filePath">The full path to the file to append to.</param>
    /// <param name="content">The single string content to append to the file.</param>
    public static void AppendAllTextAndCreateDirectory(string filePath, string content) {
        if (string.IsNullOrWhiteSpace(filePath)) {
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
        }

        try {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"[FileUtil] Directory created: {directoryPath}");
            }

            File.AppendAllText(filePath, content);
            Console.WriteLine($"[FileUtil] Content successfully appended to file: {filePath}");
        }
        catch (UnauthorizedAccessException ex) {
            Console.WriteLine($"[FileUtil Error] Access to the path '{filePath}' is denied. Please check file permissions. Details: {ex.Message}");
        }
        catch (IOException ex) {
            Console.WriteLine($"[FileUtil Error] An I/O error occurred while appending to the file. It might be in use by another process. Details: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"[FileUtil Error] An unknown error occurred: {ex.Message}");
        }
    }

    private static readonly Random _random = new Random();
    private static readonly HashSet<string> _generatedStrings = new HashSet<string>();

    /// <summary>
    /// Generates a random string of a given length that is guaranteed to be unique
    /// within the current application session.
    /// It uses a HashSet to store and check for previously generated strings to ensure uniqueness.
    /// </summary>
    /// <param name="length">The desired length of the random string.</param>
    /// <returns>A unique random string.</returns>
    public static string GenerateRandomString(int length) {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        while (true) {
            string candidate = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());

            // Attempt to add the new string to the set.
            // .Add() returns true if the item was added,
            // and false if the item was already present.
            if (_generatedStrings.Add(candidate)) {
                return candidate;
            }
            // If the string was already in the set, the loop continues and generates a new one.
        }
    }

    public static DecoParser.PrimaryContext GetPrimaryContext(DecoParser.ExpressionContext expression) {
        if (expression == null) return null;

        var orExpr = expression.or_expr();
        if (orExpr.and_expr().Length > 1) return null;

        var andExpr = orExpr.and_expr(0);
        if (andExpr.eq_expr().Length > 1) return null;

        var eqExpr = andExpr.eq_expr(0);
        if (eqExpr.rel_expr().Length > 1) return null;

        var relExpr = eqExpr.rel_expr(0);
        if (relExpr.add_expr().Length > 1) return null;

        var addExpr = relExpr.add_expr(0);
        if (addExpr.mul_expr().Length > 1) return null;

        var mulExpr = addExpr.mul_expr(0);
        if (mulExpr.unary_expr().Length > 1) return null;

        var unaryExpr = mulExpr.unary_expr(0);

        if (unaryExpr.primary() == null) return null;

        return unaryExpr.primary();
    }
}

public class Base36Counter {
    private static readonly char[] _chars = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private readonly char[] _buffer = ['0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'];
    private int _startIndex = 15; // Start Index of valid digit

    /// <summary>
    /// Gets the next base-36 string. (Without leading zeros)
    /// </summary>
    /// <param name="width">default 0. If you need the leading zeros, you should
    /// provide the total width of the base-36 string</param>
    /// <returns>The next base-36 string.</returns>
    /// <exception cref="OverflowException">Thrown when the 36-base counter has
    /// reached its maximum value.</exception>
    public string Next(int width = 0) {
        // Increment from the last digit
        for (int i = 15; i >= 0; i--) {
            char c = _buffer[i];
            int value;

            // Fast char to value conversion
            if (c <= '9')
                value = c - '0';
            else
                value = c - 'a' + 10;

            // Attempt to increment by 1
            value++;

            if (value < 36) {
                // No carry-over needed
                _buffer[i] = _chars[value];

                // Update the start index of the valid digits
                if (i < _startIndex)
                    _startIndex = i;

                // Return the string without leading zeros
                return width == 0
                    ? new string(_buffer, _startIndex, 16 - _startIndex)
                    : new string(_buffer, 16 - width, width);
            }

            // Carry-over needed, current digit resets to '0'
            _buffer[i] = '0';
        }

        // If execution reaches here, the counter is full (all 16 digits are maxed out)
        throw new OverflowException("36-base counter has overflowed.");
    }

    /// <summary>
    /// Resets the counter to its initial state ("0").
    /// </summary>
    public void Reset() {
        for (int i = 0; i < 16; i++) {
            _buffer[i] = '0';
        }
        _startIndex = 15;
    }
}