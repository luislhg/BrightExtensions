namespace BrightXaml.Extensibility;
internal static class Meta
{
    public static bool IsDebug { get; } =
#if DEBUG
        true;
#else
        false;
#endif
}
