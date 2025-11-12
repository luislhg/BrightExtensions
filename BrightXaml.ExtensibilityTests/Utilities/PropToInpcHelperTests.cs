namespace BrightXaml.Extensibility.Utilities.Tests;

[TestClass()]
public class PropToInpcHelperTests
{
    [TestMethod()]
    [DataRow("public string Name { get; set; }", "", "public", "string", "Name", "", "")]
    [DataRow("    public int Age { get; set; }", "    ", "public", "int", "Age", "", "")]
    [DataRow(" public bool IsEnabled { get; set; }", " ", "public", "bool", "IsEnabled", "", "")]
    [DataRow("  uint Ticks { get; set; }", "  ", "", "uint", "Ticks", "", "")]
    [DataRow("protected    long ShortTicks { get; set; }", "", "protected", "long", "ShortTicks", "", "")]
    [DataRow("protected ulong PositiveShortTicks { private   get; set; }", "", "protected", "ulong", "PositiveShortTicks", "private", "")]
    [DataRow("protected ulong PositiveShortTicks { internal   get; set; }", "", "protected", "ulong", "PositiveShortTicks", "internal", "")]
    [DataRow("protected ulong PositiveShortTicks { protected   get; set; }", "", "protected", "ulong", "PositiveShortTicks", "protected", "")]
    [DataRow("protected ulong PositiveShortTicks { public   get; set; }", "", "protected", "ulong", "PositiveShortTicks", "public", "")]
    [DataRow("protected ulong PositiveShortTicks { get; private set; }", "", "protected", "ulong", "PositiveShortTicks", "", "private")]
    [DataRow("protected ulong PositiveShortTicks { get;internal set; }", "", "protected", "ulong", "PositiveShortTicks", "", "internal")]
    [DataRow("protected ulong PositiveShortTicks { get;  protected  set; }", "", "protected", "ulong", "PositiveShortTicks", "", "protected")]
    [DataRow("protected ulong PositiveShortTicks { get;public    set; }", "", "protected", "ulong", "PositiveShortTicks", "", "public")]
    [DataRow("public string Name { get; set; } = \"John Winchester\";", "", "public", "string", "Name", "", "", "\"John Winchester\"")]
    [DataRow("public bool IsChecked { get; set; } = true;", "", "public", "bool", "IsChecked", "", "", "true")]
    public void GetPropertyLineDataTests(string line,
                                         string expectedIndent,
                                         string expectedAccess,
                                         string expectedType,
                                         string expectedName,
                                         string expectedGetAccess,
                                         string expectedSetAccess,
                                         string expectedDefaultValue = null)
    {
        // Arrange.
        // Act.
        var propertyLineData = PropToInpcHelper.GetPropertyLineData(line);

        // Assert.
        Assert.AreEqual(expectedIndent, propertyLineData.Indentation);
        Assert.AreEqual(expectedAccess, propertyLineData.Access);
        Assert.AreEqual(expectedType, propertyLineData.Type);
        Assert.AreEqual(expectedName, propertyLineData.Name);
        Assert.AreEqual(expectedGetAccess, propertyLineData.GetAccess);
        Assert.AreEqual(expectedSetAccess, propertyLineData.SetAccess);
        Assert.AreEqual(expectedDefaultValue, propertyLineData.DefaultValue);
    }

    [TestMethod()]
    [DataRow("public string Name { get; set; }", "private string name;", "public string Name { get => name; set => Set(ref name, value); }")]
    [DataRow("public int Age { get; set; }", "private int age;", "public int Age { get => age; set => Set(ref age, value); }")]
    [DataRow("protected bool IsEnabled { get; set; }", "private bool isEnabled;", "protected bool IsEnabled { get => isEnabled; set => Set(ref isEnabled, value); }")]
    [DataRow("private uint Ticks { get; set; }", "private uint ticks;", "private uint Ticks { get => ticks; set => Set(ref ticks, value); }")]
    [DataRow("internal long ShortTicks { get; set; }", "private long shortTicks;", "internal long ShortTicks { get => shortTicks; set => Set(ref shortTicks, value); }")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", "private string fullName = \"John Winchester\";", "public string FullName { get => fullName; set => Set(ref fullName, value); }")]
    [DataRow("public string IsChecked { get; set; } = true;", "private string isChecked = true;", "public string IsChecked { get => isChecked; set => Set(ref isChecked, value); }")]
    public void GenerateInpcPropertySetTest(string propertyText, string expectedFieldLine, string expectedPropertyLine)
    {
        // Arrange.
        // Act.
        var propertyLineData = PropToInpcHelper.GetPropertyLineData(propertyText);
        var result = PropToInpcHelper.GenerateInpcPropertySet(propertyLineData, false, false, true, "Set");

        // Assert.
        var resultLines = result.Split(Environment.NewLine);
        Assert.IsTrue(resultLines.Length == 2);
        Assert.AreEqual(expectedPropertyLine, resultLines.FirstOrDefault());
        Assert.AreEqual(expectedFieldLine, resultLines.LastOrDefault());
    }

    [TestMethod()]
    [DataRow("public string Name { get; set; }", false, "Set", "public string Name { get => field; set => Set(ref field, value); }")]
    [DataRow("public int Age { get; set; }", false, "Set", "public int Age { get => field; set => Set(ref field, value); }")]
    [DataRow("protected bool IsEnabled { get; private set; }", false, "Set", "protected bool IsEnabled { get => field; private set => Set(ref field, value); }")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", true, "Set", "public string FullName { get => field; set => Set(ref field, value); } = \"John Winchester\";")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", false, "Set", "public string FullName { get => field; set => Set(ref field, value); }")]
    public void GenerateInpcPropertySetFieldKeywordTest(string propertyText, bool preserveDefaultValue, string setMethodName, string expectedLine)
    {
        // Arrange.
        // Act.
        var propertyLineData = PropToInpcHelper.GetPropertyLineData(propertyText);
        var result = PropToInpcHelper.GenerateInpcPropertySetFieldKeyword(propertyLineData, preserveDefaultValue, setMethodName);

        // Assert.
        Assert.AreEqual(expectedLine, result);
    }
}