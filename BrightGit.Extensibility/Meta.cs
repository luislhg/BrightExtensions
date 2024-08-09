using System.Reflection;

namespace BrightGit.Extensibility;
internal static class Meta
{
    public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

    public static bool IsDebug { get; } =
#if DEBUG
        true;
#else
        false;
#endif
}
