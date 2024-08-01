using LibGit2Sharp;
using System.Reflection;

namespace BrightGit.Extensibility.Helpers;
internal static class LibGit2SharpHelper
{
    public static void RegisterNativePath(string path)
    {
        // We check first because if we overwrite after in use, it will throw an exception.
        if (GlobalSettings.NativeLibraryPath == null)
        {
            // Set the native library path for LibGit2Sharp when inside VS extension (VSIX).
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblyFolder = assemblyFolder[..^2];
            GlobalSettings.NativeLibraryPath = Path.Combine(assemblyFolder, "runtimes", "win-x64", "native");
        }
    }
}
