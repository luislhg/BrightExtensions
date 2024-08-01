namespace BrightXaml.Extensibility.Windows;

using Microsoft.VisualStudio.Extensibility.UI;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

[DataContract]
internal class ChooseItemWindowViewModel : NotifyPropertyChangedObject
{
    [DataMember]
    public ObservableCollection<string> Items { get => _items; set => SetProperty(ref _items, value); }
    private ObservableCollection<string> _items = new ObservableCollection<string>();

    [DataMember]
    public string SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
    private string _selectedItem;

    [DataMember]
    public string LabelText { get => _labelText; set => SetProperty(ref _labelText, value); }
    private string _labelText;

    public ChooseItemWindowViewModel()
    { }
}