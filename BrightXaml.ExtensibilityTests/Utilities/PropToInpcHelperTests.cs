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
    [DataRow("public string Name { get; set; }", false, "Set", "public string Name { get; set => Set(ref field, value); }")]
    [DataRow("public int Age { get; set; }", false, "Set", "public int Age { get; set => Set(ref field, value); }")]
    [DataRow("protected bool IsEnabled { get; private set; }", false, "Set", "protected bool IsEnabled { get; private set => Set(ref field, value); }")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", true, "Set", "public string FullName { get; set => Set(ref field, value); } = \"John Winchester\";")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", false, "Set", "public string FullName { get; set => Set(ref field, value); }")]
    public void GenerateInpcPropertySetFieldKeywordTest(string propertyText, bool preserveDefaultValue, string setMethodName, string expectedLine)
    {
        // Arrange.
        // Act.
        var propertyLineData = PropToInpcHelper.GetPropertyLineData(propertyText);
        var result = PropToInpcHelper.GenerateInpcPropertySetFieldKeyword(propertyLineData, preserveDefaultValue, setMethodName);

        // Assert.
        Assert.AreEqual(expectedLine, result);
    }

    [TestMethod()]
    [DataRow("public string Name { get; set; }", false, "[ObservableProperty]\r\npublic partial string Name { get; set; }")]
    [DataRow("public int Age { get; set; }", false, "[ObservableProperty]\r\npublic partial int Age { get; set; }")]
    [DataRow("protected bool IsEnabled { get; set; }", false, "[ObservableProperty]\r\nprotected partial bool IsEnabled { get; set; }")]
    [DataRow("private uint Ticks { get; set; }", false, "[ObservableProperty]\r\nprivate partial uint Ticks { get; set; }")]
    [DataRow("internal long ShortTicks { get; set; }", false, "[ObservableProperty]\r\ninternal partial long ShortTicks { get; set; }")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", true, "[ObservableProperty]\r\npublic partial string FullName { get; set; } = \"John Winchester\";")]
    [DataRow("public bool IsChecked { get; set; } = true;", true, "[ObservableProperty]\r\npublic partial bool IsChecked { get; set; } = true;")]
    [DataRow("public string FullName { get; set; } = \"John Winchester\";", false, "[ObservableProperty]\r\npublic partial string FullName { get; set; }")]
    [DataRow("protected ulong PositiveShortTicks { private get; set; }", false, "[ObservableProperty]\r\nprotected partial ulong PositiveShortTicks { private get; set; }")]
    [DataRow("protected ulong PositiveShortTicks { get; private set; }", false, "[ObservableProperty]\r\nprotected partial ulong PositiveShortTicks { get; private set; }")]
    [DataRow("protected ulong PositiveShortTicks { internal get; set; }", false, "[ObservableProperty]\r\nprotected partial ulong PositiveShortTicks { internal get; set; }")]
    [DataRow("protected ulong PositiveShortTicks { get; protected set; }", false, "[ObservableProperty]\r\nprotected partial ulong PositiveShortTicks { get; protected set; }")]
    [DataRow("    public string Name { get; set; }", false, "    [ObservableProperty]\r\n    public partial string Name { get; set; }")]
    public void GenerateInpcPropertyObservablePropertyTest(string propertyText, bool preserveDefaultValue, string expectedOutput)
    {
        // Arrange.
        // Act.
        var propertyLineData = PropToInpcHelper.GetPropertyLineData(propertyText);
        var result = PropToInpcHelper.GenerateInpcPropertyObservableProperty(propertyLineData, preserveDefaultValue);

        // Assert.
        Assert.AreEqual(expectedOutput, result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_BasicMultiLine_ReturnsCombined()
    {
        // Arrange - multi-line property with cursor on the opening brace line
        string fullText = "public string TestText\r\n{\r\n    get; set;\r\n}";
        int caretOffset = 25; // On the { line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public string TestText { get; set; }", result);
        Assert.AreEqual(0, startOffset);
        Assert.AreEqual(fullText.Length, length);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_WithIndentation_PreservesIndentation()
    {
        // Arrange - indented multi-line property
        string fullText = "    public string TestText\r\n    {\r\n        get; set;\r\n    }";
        int caretOffset = 30; // On the { line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("    public string TestText { get; set; }", result);
        Assert.AreEqual(0, startOffset);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_GetSetOnSeparateLines_ReturnsCombined()
    {
        // Arrange - get and set on separate lines
        string fullText = "public string TestText\r\n{\r\n    get;\r\n    set;\r\n}";
        int caretOffset = 27; // On the get; line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public string TestText { get; set; }", result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_WithAccessModifiers_ReturnsCombined()
    {
        // Arrange - property with access modifiers on accessors
        string fullText = "protected int Count\r\n{\r\n    private get;\r\n    set;\r\n}";
        int caretOffset = 27; // On the private get; line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("protected int Count { private get; set; }", result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_WithDefaultValue_ReturnsCombined()
    {
        // Arrange - property with default value
        string fullText = "public bool IsActive\r\n{\r\n    get; set;\r\n} = true;";
        int caretOffset = 25; // On the { line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public bool IsActive { get; set; } = true;", result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_SingleLineProperty_ReturnsNull()
    {
        // Arrange - single-line property (should not be combined)
        string fullText = "public string TestText { get; set; }";
        int caretOffset = 20; // Middle of the line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(-1, startOffset);
        Assert.AreEqual(-1, length);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_CaretOnPropertyName_FindsProperty()
    {
        // Arrange - cursor on the property declaration line
        string fullText = "public string TestText\r\n{\r\n    get; set;\r\n}";
        int caretOffset = 14; // On "TestText"

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public string TestText { get; set; }", result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_CaretOnClosingBrace_FindsProperty()
    {
        // Arrange - cursor on the closing brace line
        string fullText = "public string TestText\r\n{\r\n    get; set;\r\n}";
        int caretOffset = 42; // On the } line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public string TestText { get; set; }", result);
    }

    [TestMethod()]
    public void CombineMultiLineProperty_InClassWithOtherCode_FindsCorrectProperty()
    {
        // Arrange - property within a class with other code
        string fullText = "public class MyClass\r\n{\r\n    private int _field;\r\n\r\n    public string TestText\r\n    {\r\n        get; set;\r\n    }\r\n\r\n    public void Method() { }\r\n}";
        int caretOffset = 70; // On the property's get; line

        // Act
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("    public string TestText { get; set; }", result);
        Assert.IsTrue(startOffset > 0); // Should be after the field declaration
    }

    [TestMethod()]
    public void CombineMultiLineProperty_ProtectedInternal_ReturnsCombined()
    {
        // Arrange - property with protected internal modifier
        string fullText = "protected internal string Name\r\n{\r\n    get; set;\r\n}";
        int caretOffset = 35; // On the { line

        // Act  
        var result = PropToInpcHelper.CombineMultiLineProperty(fullText, caretOffset, out int startOffset, out int length);

        // Assert - Note: This will combine as "protected internal string Name { get; set; }"
        // The GetPropertyLineData method might have issues with "protected internal" since it only checks for single keywords
        Assert.IsNotNull(result);
        StringAssert.Contains(result, "protected");
        StringAssert.Contains(result, "get; set;");
    }
}