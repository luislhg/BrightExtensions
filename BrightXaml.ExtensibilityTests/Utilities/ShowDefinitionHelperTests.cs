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
    [DataRow("ProjectWpf.MainClass.cs", "ProjectWpf\\MainClass.cs")]
    [DataRow("ProjectWpf.MainClass.g.cs", "ProjectWpf\\MainClass.cs")]
    [DataRow("ProjectWpf.Common.MainClass.cs", "ProjectWpf.Common\\MainClass.cs")]
    [DataRow("ProjectWpf.Common.MainClass.g.cs", "ProjectWpf.Common\\MainClass.cs")]
    public void FindFileFromUriSegmentTest(string lastSegment, string expected)
    {
        expected = $"D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\{expected}";

        List<string> filesFound = new List<string>
        {
            "D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\ProjectWpf\\MainClass.cs",
            "D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\ProjectWpf.Common\\MainClass.cs",
            "D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\ProjectWpf.Common\\Pages\\MainClass.cs",
            "D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\ProjectWpf.Common.Windows\\MainClass.cs",
            "D:\\Projects Visual Studio\\Github Projects\\Bright Extensions Tests\\ProjectWpf.Common.Windows\\Pages\\MainClass.cs",
        };

        var actual = ShowDefinitionHelper.FindCorrectFile(filesFound, lastSegment);

        Assert.AreEqual(expected, actual);
    }
}