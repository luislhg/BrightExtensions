using System.Reflection;
using System.Runtime.CompilerServices;

namespace BrightXaml.ExtensibilityTests;
internal static class TestHelper
{
    internal static string ReadResource(string resourceName)
    {
        // Get the assembly where the resource is embedded.
        var assembly = Assembly.GetExecutingAssembly();

        // Create the full resource name.
        string resourceFullName = assembly.GetName().Name + "." + resourceName;

        using (Stream stream = assembly.GetManifestResourceStream(resourceFullName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException("Resource not found: " + resourceFullName);
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    internal static void WriteTestResultToFile(string text, [CallerMemberName] string callerName = null)
    {
        File.WriteAllText(callerName + ".xaml", text);
    }
}
