namespace BrightXaml.Extensibility.Utilities.Tests;

[TestClass()]
public class ViewModelHelperTests
{
    [TestMethod()]
    [DataRow("Main", "MainViewModel.cs")]
    [DataRow("MainPage", "MainViewModel.cs")]
    [DataRow("MainPage", "MainPageViewModel.cs")]
    [DataRow("MainWindow", "MainViewModel.cs")]
    [DataRow("MainWindow", "MainWindowViewModel.cs")]
    [DataRow("MyNiceMvvmClientViewEdit", "MyNiceMvvmClientViewEditViewModel.cs")]
    [DataRow("ChooseItemWindowContent", "ChooseItemWindowViewModel.cs")]
    public void GetViewModelNamePossibilitiesTest(string xamlName, string expected)
    {
        var possibilities = ViewModelHelper.GetViewModelNamePossibilities(xamlName);
        Assert.IsTrue(possibilities.Contains(expected));
    }

    [TestMethod()]
    [DataRow("MainViewModel", "MainView.xaml")]
    [DataRow("MainViewModel", "MainPage.xaml")]
    [DataRow("MainViewModel", "MainWindow.xaml")]
    [DataRow("MainViewModel", "Main.xaml")]
    [DataRow("MainWindowViewModel", "MainWindowContent.xaml")]
    public void GetViewNamePossibilitiesTest(string viewModelName, string expected)
    {
        var possibilities = ViewModelHelper.GetViewNamePossibilities(viewModelName);
        Assert.IsTrue(possibilities.Contains(expected));
    }

    [TestMethod()]
    public void RemoveCommonPathTest()
    {
        // Arrange
        List<string> filePaths = new List<string>
        {
            "C:/test/my/project/test.cs",
            "C:/test/my/project/test2.cs",
            "C:/test/my/project/test.cs",
            "C:/test/my/project2/test2.cs",
            "C:/test/my2/project/test.cs"
        };

        List<string> expected = new List<string>
        {
            "my/project/test.cs",
            "my/project/test2.cs",
            "my/project/test.cs",
            "my/project2/test2.cs",
            "my2/project/test.cs"
        };

        // Act
        List<string> result = ViewModelHelper.RemoveCommonPath(filePaths);

        // Assert
        CollectionAssert.AreEqual(expected, result);
    }
}