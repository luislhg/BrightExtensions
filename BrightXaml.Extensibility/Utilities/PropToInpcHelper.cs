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
}
