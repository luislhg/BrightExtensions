namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A command for showing a tool window.
/// </summary>
[VisualStudioContribution]
public class ProgressWindowCommand : Command
{
    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new(displayName: "Progress Window")
    {
        // Use this object initializer to set optional parameters for the command. The required parameter,
        // displayName, is set above. To localize the displayName, add an entry in .vsextension\string-resources.json
        // and reference it here by passing "%BrightXaml.Extensibility.Windows.ProgressWindowCommand.DisplayName%" as a constructor parameter.
        //Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await this.Extensibility.Shell().ShowToolWindowAsync<ProgressWindow>(activate: true, cancellationToken);
    }
}
