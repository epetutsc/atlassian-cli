using System.Text;

namespace AtlassianCli.Commands;

/// <summary>
/// Provides helper methods for content resolution in CLI command handlers.
/// </summary>
internal static class ContentResolver
{
    /// <summary>
    /// Resolves content from either a direct string value or a file path.
    /// Files are read as UTF-8 encoded text.
    /// </summary>
    /// <param name="directContent">Direct content string (optional).</param>
    /// <param name="filePath">Path to a file containing the content (optional).</param>
    /// <param name="contentName">Name of the content for error messages (e.g., "body", "description").</param>
    /// <returns>The resolved content string.</returns>
    /// <exception cref="ArgumentException">Thrown when neither or both options are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    public static string ResolveContent(string? directContent, string? filePath, string contentName)
    {
        bool hasDirectContent = !string.IsNullOrEmpty(directContent);
        bool hasFilePath = !string.IsNullOrEmpty(filePath);

        if (hasDirectContent && hasFilePath)
        {
            throw new ArgumentException($"You cannot specify both --{contentName} and --file. Please use only one.");
        }

        if (!hasDirectContent && !hasFilePath)
        {
            throw new ArgumentException($"You must specify either --{contentName} or --file.");
        }

        if (hasFilePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file does not exist: {filePath}");
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        return directContent!;
    }

    /// <summary>
    /// Resolves optional content from either a direct string value or a file path.
    /// Files are read as UTF-8 encoded text.
    /// </summary>
    /// <param name="directContent">Direct content string (optional).</param>
    /// <param name="filePath">Path to a file containing the content (optional).</param>
    /// <param name="contentOptionName">Name of the content option for error messages (e.g., "description").</param>
    /// <param name="fileOptionName">Name of the file option for error messages (e.g., "description-file").</param>
    /// <returns>The resolved content string, or null if neither is provided.</returns>
    /// <exception cref="ArgumentException">Thrown when both options are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    public static string? ResolveOptionalContent(string? directContent, string? filePath, string contentOptionName, string fileOptionName)
    {
        bool hasDirectContent = !string.IsNullOrEmpty(directContent);
        bool hasFilePath = !string.IsNullOrEmpty(filePath);

        if (hasDirectContent && hasFilePath)
        {
            throw new ArgumentException($"You cannot specify both --{contentOptionName} and --{fileOptionName}. Please use only one.");
        }

        if (!hasDirectContent && !hasFilePath)
        {
            return null;
        }

        if (hasFilePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified file does not exist: {filePath}");
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        return directContent;
    }
}
