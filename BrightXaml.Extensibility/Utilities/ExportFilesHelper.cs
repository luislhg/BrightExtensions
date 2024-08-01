using System.Text;

namespace BrightXaml.Extensibility.Utilities;
public static class ExportFilesHelper
{
    public static List<string> GetFilesFromDir(string directoryPath, bool includeSubDirs, List<string> fileExtensions)
    {
        var files = new List<string>();
        // Ensure the directory exists.
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
        }

        // Iterate over each file extension and get the files.
        foreach (var extension in fileExtensions)
        {
            var matchedFiles = Directory.GetFiles(directoryPath, $"*{extension}", includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            files.AddRange(matchedFiles);
        }

        // Sort the files to ensure related files are together.
        files.Sort();

        return files;
    }

    public static async Task<string> ReadFilesContentAsync(List<string> filenames, bool useMarkdown, bool includeFileName)
    {
        // StringBuilder to accumulate file contents.
        var allContents = new StringBuilder(filenames.Count * 256);

        foreach (string file in filenames)
        {
            string extension = Path.GetExtension(file).ToLowerInvariant();
            string language = GetMarkdownLanguage(extension);
            string commentStart, commentEnd;

            // Determine the comment syntax based on file extension
            GetCommentSyntax(extension, out commentStart, out commentEnd);

            // Start of Markdown code block for the specific file type.
            if (useMarkdown && !string.IsNullOrEmpty(language))
                allContents.AppendLine($"```{language}");

            // Read and append each file's content.
            string content = await File.ReadAllTextAsync(file);
            if (!string.IsNullOrEmpty(commentStart))
            {
                if (includeFileName)
                {
                    allContents.AppendLine($"{commentStart} File: {Path.GetFileName(file)} {commentEnd}");
                }
            }
            allContents.AppendLine(content);

            // End of Markdown code block.
            if (useMarkdown && !string.IsNullOrEmpty(language))
                allContents.AppendLine("```");

            allContents.AppendLine();
        }

        return allContents.ToString();
    }

    private static string GetMarkdownLanguage(string extension)
    {
        return extension switch
        {
            ".cs" => "csharp",
            ".xaml" => "xml",
            ".xml" => "xml",
            ".html" => "html",
            ".css" => "css",
            ".js" => "javascript",
            _ => string.Empty,
        };
    }

    private static void GetCommentSyntax(string extension, out string commentStart, out string commentEnd)
    {
        switch (extension)
        {
            case ".cs":
            case ".js":
                commentStart = "//";
                commentEnd = "";
                break;
            case ".xaml":
            case ".xml":
            case ".html":
                commentStart = "<!--";
                commentEnd = " -->";
                break;
            case ".css":
                commentStart = "/*";
                commentEnd = " */";
                break;
            default:
                commentStart = "//";
                commentEnd = "";
                break;
        }
    }
}
