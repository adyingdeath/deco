using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Provides utility methods for common file operations, including creating missing directories.
/// </summary>
public static class Util {

    private static readonly Random _random = new Random();

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

    public static string GenerateRandomString(int length) {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}