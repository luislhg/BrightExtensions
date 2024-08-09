using System.Reflection;

namespace BrightXaml.Extensibility;
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
