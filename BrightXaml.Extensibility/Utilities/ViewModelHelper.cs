namespace BrightXaml.Extensibility.Utilities;
public static class ViewModelHelper
{
    public static List<string> GetViewModelNamePossibilities(string xamlName)
    {
        const string suffix = "ViewModel.cs";
        string treatedName = xamlName;
        var possibilities = new List<string>();

        // Remove the "Page" or "View" suffix from the XAML file name.
        if (xamlName.EndsWith("page", StringComparison.InvariantCultureIgnoreCase) ||
            xamlName.EndsWith("view", StringComparison.InvariantCultureIgnoreCase))
        {
            treatedName = treatedName.Substring(0, xamlName.Length - 4);
        }
        // Remove the "Window" suffix from the XAML file name.
        else if (xamlName.EndsWith("window", StringComparison.InvariantCultureIgnoreCase))
        {
            treatedName = treatedName.Substring(0, xamlName.Length - 6);
        }
        // Remove the "Content" suffix from the XAML file name (only VS Extensibility uses this).
        else if (xamlName.EndsWith("content", StringComparison.InvariantCultureIgnoreCase))
        {
            treatedName = treatedName.Substring(0, xamlName.Length - 7);
        }

        // Return the list of possibilities.
        possibilities.Add(treatedName + suffix);
        possibilities.Add(xamlName + suffix);

        return possibilities;
    }

    public static List<string> GetViewNamePossibilities(string viewModelName)
    {
        const string suffixXaml = ".xaml";
        string treatedName = viewModelName.Replace("ViewModel", string.Empty, StringComparison.InvariantCultureIgnoreCase);
        var possibilities = new List<string>
        {
            // Add different suffixes to generate possible view names.
            treatedName + "View" + suffixXaml,
            treatedName + "Page" + suffixXaml,
            treatedName + "Window" + suffixXaml,
            treatedName + "Content" + suffixXaml, // (only VS Extensibility uses this).
            treatedName + suffixXaml
        };

        return possibilities;
    }

    public static List<string> RemoveCommonPath(List<string> filePaths)
    {
        if (filePaths == null || filePaths.Count == 0)
        {
            return new List<string>();
        }

        string commonPath = FindCommonPath(filePaths);

        return filePaths.Select(path => path.Replace(commonPath, string.Empty)).ToList();
    }

    private static string FindCommonPath(List<string> paths)
    {
        if (paths.Count == 1)
        {
            return paths[0];
        }

        string[] separatedPaths = paths[0].Split(new[] { '/', '\\' });

        for (int pathIndex = 1; pathIndex < paths.Count; pathIndex++)
        {
            string[] nextPath = paths[pathIndex].Split(new[] { '/', '\\' });

            int minLength = Math.Min(separatedPaths.Length, nextPath.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (!separatedPaths[i].Equals(nextPath[i], StringComparison.OrdinalIgnoreCase))
                {
                    separatedPaths = separatedPaths.Take(i).ToArray();
                    break;
                }
            }
        }

        return string.Join("/", separatedPaths) + "/";
    }

    public static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
    {
        DirectoryInfo parentDirInfo = dirInfo.Parent;
        if (null == parentDirInfo)
            return dirInfo.Name;

        return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                            parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
    }

    public static string GetProperFilePathCapitalization(string filename)
    {
        FileInfo fileInfo = new FileInfo(filename);
        DirectoryInfo dirInfo = fileInfo.Directory;

        return Path.Combine(GetProperDirectoryCapitalization(dirInfo),
                            dirInfo.GetFiles(fileInfo.Name)[0].Name);
    }
}
