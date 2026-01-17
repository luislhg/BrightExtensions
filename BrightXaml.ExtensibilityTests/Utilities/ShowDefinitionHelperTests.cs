namespace BrightXaml.Extensibility.Utilities.Tests;

[TestClass()]
public class ShowDefinitionHelperTests
{
    [TestMethod()]
    [DataRow("<ListView ItemsSource=\"{Binding Containers}\"", 37, "Containers")]
    [DataRow("<ListView ItemsSource=\"{Binding Path.Containers}\"", 37, "Containers")]
    [DataRow(" Path.Containers ", 8, "Containers")]
    public void GetWordAtCaretTest(string lineContent, int caretOffset, string expected)
    {
        string actual = ShowDefinitionHelper.GetWordAtCaret(lineContent, caretOffset);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow("WinDivert.MainWindowViewModel", "WinDivert\\MainWindowViewModel.cs")]
    [DataRow("WinDivert.Wpf.MainWindowViewModel", "WinDivert.Wpf\\MainWindowViewModel.cs")]
    [DataRow("WinDivert.Common.Wpf.MainWindowViewModel", "WinDivert.Common.Wpf\\MainWindowViewModel.cs")]
    [DataRow("MainWindowViewModel", "MainWindowViewModel.cs")]
    public void FixSegmentPathTest(string segmentInput, string expected)
    {
        string actual = ShowDefinitionHelper.FixSegmentPath(segmentInput);
        Assert.AreEqual(expected, actual);
    }
}