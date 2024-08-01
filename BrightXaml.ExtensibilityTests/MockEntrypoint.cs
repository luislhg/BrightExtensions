using Microsoft.VisualStudio.Extensibility;

namespace BrightXaml.ExtensibilityTests;
[VisualStudioContribution]
internal class MockEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => null;
}
