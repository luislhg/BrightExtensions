namespace BrightXaml.Extensibility.Windows;

using BrightXaml.Extensibility.Models;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

[DataContract]
internal class HelpWindowViewModel : NotifyPropertyChangedObject
{
    [DataMember]
    public string Text { get => _text; set => SetProperty(ref _text, value); }
    private string _text = string.Empty;

    [DataMember]
    public ObservableCollection<CommandInfo> CommandsInfo { get => _commandsInfo; set => SetProperty(ref _commandsInfo, value); }
    private ObservableCollection<CommandInfo> _commandsInfo;

    [DataMember]
    public AsyncCommand RefreshCommand { get; }

    public HelpWindowViewModel()
    {
        RefreshCommand = new AsyncCommand((parameter, clientContext, cancellationToken) =>
        {
            InitializeCommandsInfo();
            return Task.CompletedTask;
        });
        RefreshCommand.CanExecute = Meta.IsDebug;

        InitializeCommandsInfo();
    }

    private void InitializeCommandsInfo()
    {
        CommandsInfo = new ObservableCollection<CommandInfo>
        {
            new() { Name = "Clean Solution Bin and Obj", Description = "Clean 'bin' and 'obj' folders from all projects in the solution" },
            new() { Name = "Kill XAML Designer Process", Description = "Kills the WpfSurface.exe process, VS can auto reload it later" },
            //new() { Name = "Extract Classes Being Used", Description = "Copy to the clipboard all C# classes being referenced by the current class" },
            //new() { Name = "Extract Directory", Description = "Copy to the clipboard all coding files from the current directory" },
            new() { Name = "Format Xaml", Description = "Format the current XAML file (preserves tags position and indentation)" },
            new() { Name = "Property To INPC", Description = "Convert regular properties to INotifyPropertyChanged properties" },
            new() { Name = "Automatically Go To Binding Definition", Description = "When pressing F12, if a SourceGenerator file is opened by VS,\nautomatically open the actual command in the ViewModel" },
            new() { Name = "Show View/ViewModel", Description = "Display the view of the current ViewModel and vice-versa" },
            //new() { Name = "Show View", Description = "Display the view of the current ViewModel" },
            //new() { Name = "Show View Model", Description = "Display the ViewModel of the current View (.xaml | .xaml.cs)" }
        };
    }
}
