using System.Text;
using System.Text.RegularExpressions;

namespace BrightXaml.Extensibility.Utilities;
public static class PropToInpcHelper
{
    public static PropertyLineData GetPropertyLineData(string line)
    {
        // Check if line is null.
        if (line == null)
            throw new ArgumentException("Property line cannot be null.");

        // Check if it is a property line.
        if (!line.Contains("get;") || !line.Contains("set;"))
            throw new ArgumentException("Invalid property line. It must contain auto 'get;' and 'set;'.");

        if (!line.Contains("{") || !line.Contains("}"))
            throw new ArgumentException("Invalid property line. It must contain '{' and '}' (single line prop).");

        // Get indentation.
        string indent = new string(' ', line.Length - line.TrimStart().Length);

        // Remove extra spaces
        line = line.Replace("  ", " ").Replace("  ", " ");

        // Split the line.
        var parts = line.TrimStart().Split(' ');
        if (parts.Length >= 3)
        {
            var propertyData = new PropertyLineData() { Indentation = indent };

            // If it has an access modifier.
            if (parts[0] == "public" || parts[0] == "private" || parts[0] == "protected" || parts[0] == "internal")
            {
                propertyData.Access = parts[0];
                propertyData.Type = parts[1];
                propertyData.Name = parts[2];
            }
            else
            {
                propertyData.Access = string.Empty;
                propertyData.Type = parts[0];
                propertyData.Name = parts[1];
            }

            // Extract get and set accessors.
            var getMatch = Regex.Match(line, @"(\b(public|private|protected|internal)\s+)?get;");
            propertyData.GetAccess = getMatch.Success && !string.IsNullOrEmpty(getMatch.Groups[2].Value) ? getMatch.Groups[2].Value : string.Empty;

            var setMatch = Regex.Match(line, @"(\b(public|private|protected|internal)\s+)?set;");
            propertyData.SetAccess = setMatch.Success && !string.IsNullOrEmpty(setMatch.Groups[2].Value) ? setMatch.Groups[2].Value : string.Empty;

            // Extract default value.
            var defaultValueMatch = Regex.Match(line, @"=\s*(.*?);");
            propertyData.DefaultValue = defaultValueMatch.Success ? defaultValueMatch.Groups[1].Value : null;

            return propertyData;
        }

        return null;
    }

    /// <summary>
    /// Combines a multi-line property declaration into a single line for parsing.
    /// Preserves the indentation from the first line.
    /// </summary>
    /// <param name="fullText">The complete text content of the document.</param>
    /// <param name="caretOffset">The current caret position offset.</param>
    /// <param name="replaceStartOffset">Output: The starting offset of the property in the document.</param>
    /// <param name="replaceLength">Output: The length of the property text to replace.</param>
    /// <returns>A single-line representation of the property, or null if not found.</returns>
    public static string CombineMultiLineProperty(string fullText, int caretOffset, out int replaceStartOffset, out int replaceLength)
    {
        replaceStartOffset = -1;
        replaceLength = -1;

        if (string.IsNullOrEmpty(fullText))
            return null;

        var lines = fullText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // Find the line number of the current line.
        int currentLineIndex = 0;
        int currentOffset = 0;
        foreach (var l in lines)
        {
            if (currentOffset + l.Length >= caretOffset)
                break;
            currentOffset += l.Length + Environment.NewLine.Length;
            currentLineIndex++;
        }

        // Get the current line content.
        string currentLine = currentLineIndex < lines.Length ? lines[currentLineIndex] : string.Empty;

        // If current line already has both braces, return null (single line case).
        if (currentLine.Contains("{") && currentLine.Contains("}"))
            return null;

        // If current line is empty or only contains whitespace, return null.
        if (string.IsNullOrWhiteSpace(currentLine))
            return null;

        // Search upwards to find the start of the property (line with property declaration).
        // A property declaration line has: access_modifier type name OR type name (without braces on same line).
        int startLineIndex = currentLineIndex;
        bool foundPropertyDeclaration = false;
        for (int i = currentLineIndex; i >= 0 && i >= currentLineIndex - 10; i--)
        {
            if (i < lines.Length)
            {
                var line = lines[i].TrimStart();
                // Look for property declaration: access modifier followed by type and name
                // Ensure we're not on a line that's just an accessor (contains get; or set;)
                bool hasAccessModifier = line.StartsWith("public ") || line.StartsWith("private ") ||
                                        line.StartsWith("protected ") || line.StartsWith("internal ");
                bool hasGetOrSet = line.Contains("get;") || line.Contains("set;");

                // Skip lines that are class/method declarations (contain class, void, etc.)
                bool isClassOrMethod = line.Contains(" class ") || line.Contains("void ") || 
                                       line.Contains("(") || line.Contains(")");

                if (hasAccessModifier && !hasGetOrSet && !isClassOrMethod)
                {
                    startLineIndex = i;
                    foundPropertyDeclaration = true;
                    break;
                }
            }
        }

        // If we didn't find a property declaration, return null.
        if (!foundPropertyDeclaration)
            return null;

        // Search downwards to find the closing brace.
        int endLineIndex = currentLineIndex;
        bool foundOpenBrace = currentLine.Contains("{");
        bool foundCloseBrace = currentLine.Contains("}");

        for (int i = startLineIndex; i < lines.Length && i <= startLineIndex + 10; i++)
        {
            if (lines[i].Contains("{"))
                foundOpenBrace = true;
            if (lines[i].Contains("}"))
            {
                foundCloseBrace = true;
                endLineIndex = i;
                break;
            }
        }

        // Return null if we didn't find both braces.
        if (!foundOpenBrace || !foundCloseBrace)
            return null;

        // Verify that the current line is within the property range (startLineIndex to endLineIndex).
        if (currentLineIndex < startLineIndex || currentLineIndex > endLineIndex)
            return null;

        // Combine lines.
        var propertyLines = new List<string>();
        for (int i = startLineIndex; i <= endLineIndex; i++)
        {
            if (i < lines.Length)
                propertyLines.Add(lines[i]);
        }

        // Combine into single line for parsing, preserving first line's indentation.
        string indent = new string(' ', propertyLines[0].Length - propertyLines[0].TrimStart().Length);
        var combined = string.Join(" ", propertyLines.Select(l => l.Trim()));
        string propertyText = indent + combined;

        // Validate that the combined text is actually an auto-property (must contain get; and set;).
        if (!propertyText.Contains("get;") || !propertyText.Contains("set;"))
            return null;

        // Calculate the offset and length to replace.
        replaceStartOffset = 0;
        for (int i = 0; i < startLineIndex; i++)
        {
            replaceStartOffset += lines[i].Length + Environment.NewLine.Length;
        }

        replaceLength = 0;
        for (int i = startLineIndex; i <= endLineIndex; i++)
        {
            replaceLength += lines[i].Length;
            if (i < endLineIndex)
                replaceLength += Environment.NewLine.Length;
        }

        return propertyText;
    }

    public static string GenerateInpcPropertySetFieldKeyword(PropertyLineData property, bool preserveDefaultValue, string setMethodName)
    {
        // If property.Name starts with lower case, throw an exception.
        if (string.IsNullOrWhiteSpace(property.Name) || char.IsLower(property.Name[0]))
            throw new ArgumentException("Property name must start with an upper case letter.");

        // Set default set method name.
        if (string.IsNullOrWhiteSpace(setMethodName))
            setMethodName = "SetProperty";

        var sb = new StringBuilder();

        string getAccessor = string.IsNullOrEmpty(property.GetAccess) ? string.Empty : property.GetAccess + " ";
        string setAccessor = string.IsNullOrEmpty(property.SetAccess) ? string.Empty : property.SetAccess + " ";

        // Property declaration using the 'field' contextual keyword.
        sb.Append($"{property.Indentation}{property.Access} {property.Type} {property.Name}");
        sb.Append(" {");
        sb.Append($" {getAccessor}get;");
        sb.Append($" {setAccessor}set => {setMethodName}(ref field, value);");
        sb.Append(" }");

        // Optional default value becomes an auto-property initializer.
        if (preserveDefaultValue && !string.IsNullOrWhiteSpace(property.DefaultValue))
            sb.Append($" = {property.DefaultValue};");

        return sb.ToString();
    }

    public static string GenerateInpcPropertyObservableProperty(PropertyLineData property, bool preserveDefaultValue)
    {
        // If property.Name starts with lower case, throw an exception.
        if (string.IsNullOrWhiteSpace(property.Name) || char.IsLower(property.Name[0]))
            throw new ArgumentException("Property name must start with an upper case letter.");

        var sb = new StringBuilder();

        string getAccessor = string.IsNullOrEmpty(property.GetAccess) ? string.Empty : property.GetAccess + " ";
        string setAccessor = string.IsNullOrEmpty(property.SetAccess) ? string.Empty : property.SetAccess + " ";

        // Add [ObservableProperty] attribute.
        sb.AppendLine($"{property.Indentation}[ObservableProperty]");

        // Add property declaration with partial modifier.
        sb.Append($"{property.Indentation}{property.Access} partial {property.Type} {property.Name}");
        sb.Append(" {");
        sb.Append($" {getAccessor}get;");
        sb.Append($" {setAccessor}set;");
        sb.Append(" }");

        // Optional default value as an auto-property initializer.
        if (preserveDefaultValue && !string.IsNullOrWhiteSpace(property.DefaultValue))
            sb.Append($" = {property.DefaultValue};");

        return sb.ToString();
    }

    public static string GenerateInpcPropertySet(PropertyLineData property, bool addFieldAbove, bool addFieldUnderscore, bool preserveDefaultValue, string setMethodName)
    {
        // If property.Name starts with lower case, throw an exception.
        if (char.IsLower(property.Name[0]))
            throw new ArgumentException("Property name must start with an upper case letter.");

        // Set default set method name.
        if (string.IsNullOrWhiteSpace(setMethodName))
            setMethodName = "SetProperty";

        var sb = new StringBuilder();

        // Add default value if it is present.
        string fieldValue = (preserveDefaultValue && !string.IsNullOrWhiteSpace(property.DefaultValue))
                          ? $" = {property.DefaultValue}"
                          : string.Empty;
        string fieldName = (addFieldUnderscore ? "_" : string.Empty) + char.ToLower(property.Name[0]) + property.Name[1..];
        string fieldLine = $"{property.Indentation}private {property.Type} {fieldName}{fieldValue};";
        string getAccessor = string.IsNullOrEmpty(property.GetAccess) ? string.Empty : property.GetAccess + " ";
        string setAccessor = string.IsNullOrEmpty(property.SetAccess) ? string.Empty : property.SetAccess + " ";

        // Add field if it is not below.
        if (addFieldAbove)
            sb.AppendLine($"{fieldLine}");

        // Add property.
        sb.Append($"{property.Indentation}{property.Access} {property.Type} {property.Name}");
        sb.Append(" {");
        sb.Append($" {getAccessor}get => {fieldName};");
        sb.Append($" {setAccessor}set => {setMethodName}(ref {fieldName}, value);");
        sb.Append(" }");

        // Add field if it is below.
        if (!addFieldAbove)
        {
            sb.AppendLine();
            sb.Append($"{fieldLine}");
        }

        return sb.ToString();
    }

    public static string GenerateInpcProperty(PropertyLineData property, bool addFieldAbove, bool addFieldUnderscore, string notifyMethodName)
    {
        // If property.Name starts with lower case, throw an exception.
        if (char.IsLower(property.Name[0]))
            throw new ArgumentException("Property name must start with an upper case letter.");

        // Set default notify method name.
        if (string.IsNullOrWhiteSpace(notifyMethodName))
            notifyMethodName = "OnPropertyChanged";

        var sb = new StringBuilder();
        string fieldName = (addFieldUnderscore ? "_" : string.Empty) + char.ToLower(property.Name[0]) + property.Name[1..];
        string fieldLine = $"{property.Indentation}private {property.Type} {fieldName};";

        if (addFieldAbove)
            sb.AppendLine(fieldLine);

        // Add property.
        sb.AppendLine(property.Indentation + $"{property.Access} {property.Type} {property.Name}");
        sb.AppendLine(property.Indentation + "{");
        sb.AppendLine(property.Indentation + $"    get => {fieldName};");
        sb.AppendLine(property.Indentation + "    set");
        sb.AppendLine(property.Indentation + "    {");
        sb.AppendLine(property.Indentation + $"        {fieldName} = value;");
        sb.AppendLine(property.Indentation + $"        {notifyMethodName}();");
        sb.AppendLine(property.Indentation + "    }");
        sb.AppendLine(property.Indentation + "}");

        if (!addFieldAbove)
            sb.AppendLine(fieldLine);

        return sb.ToString();
    }

    // TODO: WIP.
    //public static string GenerateInpcPropertyCompact(string propertyName, string propertyType)
    //{
    //    string fieldName = char.ToLower(propertyName[0]) + propertyName[1..];

    //    var sb = new StringBuilder();
    //    sb.AppendLine($"private {propertyType} {fieldName};");
    //    sb.AppendLine($"public {propertyType} {propertyName}");
    //    sb.AppendLine("{");
    //    sb.AppendLine($"    get => this.{fieldName};");
    //    sb.AppendLine("    set");
    //    sb.AppendLine("    {");
    //    sb.AppendLine($"        this.{fieldName} = value;");
    //    sb.AppendLine("        this.OnPropertyChanged();");
    //    sb.AppendLine("    }");
    //    sb.AppendLine("}");
    //    return sb.ToString();
    //}

    #region Auxiliary methods

    internal static string GetInUseSetMethodName(string text)
    {
        if (text.Contains("set => Set(ref "))
            return "Set";
        else if (text.Contains("set => SetProperty(ref "))
            return "SetProperty";

        return null;
    }

    internal static bool GetInUseFieldKeyword(string text)
    {
        return text.Contains("(ref field, value)");
    }

    internal static bool GetInUseObservableProperty(string text)
    {
        return text.Contains("[ObservableProperty]");
    }

    internal static bool GetInUseBackingField(string text, string setMethodName)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Handles both:
        //   set => SetProperty(ref _myProperty, value);
        //   set { SetProperty(ref _myProperty, value); }
        var pattern = @"set\s*(?:=>|\{)\s*" +
                      Regex.Escape(setMethodName) +
                      @"\s*\(\s*ref\s+(?<name>\w+)";

        var match = Regex.Match(text, pattern);
        if (!match.Success)
            return false;

        var name = match.Groups["name"].Value;
        // We only want a "real" backing field, not the literal "field"
        return !string.Equals(name, "field", StringComparison.Ordinal);
    }

    #endregion
}
