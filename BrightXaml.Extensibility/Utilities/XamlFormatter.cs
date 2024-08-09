using System.Text;
using System.Text.RegularExpressions;

namespace BrightXaml.Extensibility.Utilities;
public static partial class XamlFormatter
{
    public static async Task FormatFileAsync(string filePath, CancellationToken cancellationToken, int endingTagSpaces, int closingTagSpaces)
    {
        // Read the file content.
        var textContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

        // Format XAML.
        var formattedXaml = FormatXaml(textContent, endingTagSpaces, closingTagSpaces);

        // Apply formatted XAML to the file.
        await File.WriteAllTextAsync(filePath, formattedXaml, Encoding.UTF8, cancellationToken);
    }

    public static string FormatXaml(string xaml, int endingTagSpaces, int closingTagSpaces)
    {
        var formatted = new List<string>();

        const int indentSize = 4;
        bool useWindowsLineEnding = xaml.Contains("\r\n");
        //string xaml = SplitTagsPerLine(xaml);
        var lines = SplitLines(xaml);
        int indentLevel = 0;
        bool isInsideComment = false;

        var lastTagStartLine = string.Empty;
        var lastTagStartLineIndent = string.Empty;
        var lastTagStartLineAttribute = string.Empty;
        var lastTagStartLineAttributeSpaces = string.Empty;
        var lastTagStartLineHasAttributes = false;

        var lastLine = string.Empty;
        var lastLineIndent = string.Empty;
        var lastLineAttribute = string.Empty;
        var lastLineAttributeSpaces = string.Empty;

        foreach (var line in lines)
        {
            // Ignore the XML declaration line for formatting (usually MAUI has this).
            if (line.Equals("<?xml version=\"1.0\" encoding=\"utf-8\" ?>", StringComparison.InvariantCultureIgnoreCase))
            {
                formatted.Add(line);
                continue;
            }

            // Clean the line from extra spaces between attributes.
            string cleanedLine = RemoveExtraSpacesBetweenAttributesLine(line);
            cleanedLine = cleanedLine.TrimStart();
            cleanedLine = cleanedLine.TrimEnd();

            // Keep track of tag and comment lines.
            bool isTagStartLine = line.TrimStart().StartsWith("<");
            bool isTagEndLine = line.TrimEnd().EndsWith(">");
            bool isCommentStartLine = line.TrimStart().StartsWith("<!--");
            bool isCommentEndLine = line.TrimEnd().EndsWith("-->");

            // Only process non-empty lines.
            if (!string.IsNullOrWhiteSpace(cleanedLine))
            {
                if (!isInsideComment)
                {
                    // Decrease the indent level if the line ends a tag.
                    if (cleanedLine.StartsWith("</"))
                    {
                        indentLevel--;
                        lastLineAttributeSpaces = new string(' ', indentLevel * indentSize);
                    }

                    // Add the line to the formatted list.
                    if (isTagStartLine && !isTagEndLine)
                    {
                        cleanedLine = new string(' ', indentLevel * indentSize) + cleanedLine;
                        formatted.Add(cleanedLine);

                        // Update the last tag line variables.
                        lastTagStartLine = cleanedLine;
                        lastTagStartLineIndent = Regex.Match(cleanedLine, @"^\s*").Value;
                        lastTagStartLineAttribute = cleanedLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        lastTagStartLineAttributeSpaces = new string(' ', lastTagStartLineAttribute.Length + 1);
                        lastTagStartLineHasAttributes = cleanedLine.Contains("=");
                        //lastTagStartLineHasAttributes = true;
                    }
                    else
                    {
                        // Copy the indentation from the last tag (Align with header style).
                        if (lastTagStartLineHasAttributes)
                        {
                            formatted.Add(lastTagStartLineIndent + lastTagStartLineAttributeSpaces + cleanedLine);
                        }
                        // Copy the indentation from the last line.
                        else
                        {
                            if (isTagStartLine)
                                formatted.Add(lastLineIndent + lastLineAttributeSpaces + cleanedLine);
                            else
                                formatted.Add(lastLineAttributeSpaces + new string(' ', indentSize) + cleanedLine);
                        }
                    }
                }
                // Handle comment lines.
                else
                {
                    // Keep the comment indentation.
                    formatted.Add(line.Replace("\t", new string(' ', indentSize)));
                }

                // Reset the inside comment flag if the line ends a comment.
                if (isCommentStartLine)
                    isInsideComment = true;

                // Reset the last tag line variables if the line ends a tag.
                if (!isTagStartLine && isTagEndLine)
                {
                    lastTagStartLine = string.Empty;
                    lastTagStartLineIndent = string.Empty;
                    lastTagStartLineAttribute = string.Empty;
                    lastTagStartLineAttributeSpaces = string.Empty;
                    lastTagStartLineHasAttributes = false;
                }

                if (!isInsideComment)
                {
                    // Increase the indent level if the line starts a new tag and doesn't close inline.
                    if (cleanedLine.EndsWith(">") && !cleanedLine.EndsWith("/>") && !cleanedLine.Contains("</") && !isCommentStartLine)
                        indentLevel++;
                }

                // Reset the inside comment flag if the line ends a comment.
                if (isCommentEndLine)
                    isInsideComment = false;
            }
            else
            {
                // Add the empty line to the formatted list.
                formatted.Add(cleanedLine);
            }

            // Update the last line variables.
            lastLineIndent = Regex.Match(cleanedLine, @"^\s*").Value;
            lastLineAttribute = Regex.Match(cleanedLine, @"^\s*<\w+\s+").Value;
            lastLineAttributeSpaces = new string(' ', indentLevel * indentSize);
        }

        return string.Join(Environment.NewLine, formatted);
    }

    // Input:  <MyValue Text=\"ahaha\"     Color=\"asd\">
    // Output: <MyValue Text=\"ahaha\" Color=\"asd\">
    public static string RemoveExtraSpacesBetweenAttributesLine(string xml, int endingTagSpaces = -1, int closingTagSpaces = -1)
    {
        // Return the input if it's null or empty.
        if (string.IsNullOrWhiteSpace(xml))
            return xml;

        // Return the input if it has a comment.
        if (xml.TrimStart().StartsWith("<!--"))
            return xml;

        // Find the tag start index.
        int tagStart = xml.IndexOf('<');

        // Find the first non-space index.
        if (tagStart < 0)
        {
            tagStart = 0; // Reset tagStart to search from the beginning of the string.
            while (tagStart < xml.Length && char.IsWhiteSpace(xml[tagStart]))
                tagStart++;

            // Check if we reached the end of the string without finding a non-space character.
            if (tagStart == xml.Length)
                tagStart = -1;
        }

        var prefix = xml.Substring(0, tagStart);
        var content = xml.Substring(tagStart);

        var newContent = new StringBuilder();
        var attributes = content.Split('"');
        for (int i = 0; i < attributes.Length; i++)
        {
            var text = attributes[i];
            if (i % 2 == 0)
            {
                if (text.TrimEnd().EndsWith("/>") && closingTagSpaces >= 0)
                {
                    newContent.Append(SpacesRegex().Replace(" " + text, new string(' ', closingTagSpaces)));
                }
                else if (text.TrimEnd().EndsWith(">") && !text.TrimEnd().EndsWith("/>") && endingTagSpaces >= 0)
                {
                    newContent.Append(SpacesRegex().Replace(" " + text, new string(' ', endingTagSpaces)));
                }
                else
                {
                    newContent.Append(SpacesRegex().Replace(text, " "));
                }
            }
            else
            {
                newContent.Append(text);
            }

            if (i < attributes.Length - 1)
            {
                newContent.Append('"');
            }
        }

        return prefix + newContent.ToString();
    }

    public static string SplitTagsPerLine(string xaml)
    {
        var formatted = new List<string>();

        var lines = SplitLines(xaml);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Check if the line contains multiple /> or </.
            var split = line.Split(new string[] { "/>", "</" }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                var firstIndex = line.IndexOf("/>");
                var secondIndex = line.IndexOf("</");
                var indexOf = firstIndex < secondIndex ? firstIndex : secondIndex;

                // Add the first part of the split line.
                formatted.Add(line.Substring(0, indexOf + 2));

                // Add the rest to the next line.
                formatted.Add(line.Substring(indexOf + 2).Trim());
            }
            else
            {
                formatted.Add(line);
            }
        }

        return string.Join(Environment.NewLine, formatted);
    }

    public static string[] SplitLines(string input)
    {
        // Regular expression to split by '\r\n', '\r', or '\n'.
        var regex = new Regex(@"\r\n|\r|\n");

        // Split the input string and return the array of lines.
        return regex.Split(input);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();
}
