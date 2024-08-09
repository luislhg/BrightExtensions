using BrightXaml.ExtensibilityTests;
using System.Diagnostics;

namespace BrightXaml.Extensibility.Utilities.Tests;
[TestClass()]
public class XamlFormatterTests
{
    [TestMethod]
    [DataRow("Resources.SingleLine01_Bad.xml", "Resources.SingleLine01_Good.xml")]
    [DataRow("Resources.SingleLine02_Bad.xml", "Resources.SingleLine02_Good.xml")]
    [DataRow("Resources.SingleLine03_Bad.xml", "Resources.SingleLine03_Good.xml")]
    [DataRow("Resources.SingleLine04_Bad.xml", "Resources.SingleLine04_Good.xml")]
    //[DataRow("Resources.SingleLine05_Bad.xml", "Resources.SingleLine05_Good.xml")]
    [DataRow("Resources.SingleLineComment01_Bad.xml", "Resources.SingleLineComment01_Good.xml")]
    public void FormatXaml_SingleLine(string fileBad, string fileGood)
    {
        var input = TestHelper.ReadResource(fileBad);
        var expected = TestHelper.ReadResource(fileGood);

        // Replace tabs with spaces.
        expected = expected.Replace("\t", "    ");

        string actual = XamlFormatter.FormatXaml(input, -1, -1);
        TestHelper.WriteTestResultToFile(actual);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow("Resources.MultipleLine01_Bad.xml", "Resources.MultipleLine01_Good.xml")]
    [DataRow("Resources.MultipleLine02_Bad.xml", "Resources.MultipleLine02_Good.xml")]
    [DataRow("Resources.MultipleLine03_Bad.xml", "Resources.MultipleLine03_Good.xml")]
    [DataRow("Resources.MultipleLine04_Bad.xml", "Resources.MultipleLine04_Good.xml")]
    [DataRow("Resources.MultipleLine05_Bad.xml", "Resources.MultipleLine05_Good.xml")]
    [DataRow("Resources.MultipleLine06_Bad.xml", "Resources.MultipleLine06_Good.xml")]
    //[DataRow("Resources.MultipleLine07_Bad.xml", "Resources.MultipleLine07_Good.xml")]
    //[DataRow("Resources.MultipleLine08_Bad.xml", "Resources.MultipleLine08_Good.xml")]
    public void FormatXaml_MultipleLine(string fileBad, string fileGood)
    {
        var input = TestHelper.ReadResource(fileBad);
        var expected = TestHelper.ReadResource(fileGood);

        // Replace tabs with spaces.
        expected = expected.Replace("\t", "    ");

        string actual = XamlFormatter.FormatXaml(input, -1, -1);
        TestHelper.WriteTestResultToFile(actual);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow("Resources.FullDocument01_Bad.xml", "Resources.FullDocument01_Good.xml")]
    [DataRow("Resources.FullDocument02_Bad.xml", "Resources.FullDocument02_Good.xml")]
    [DataRow("Resources.FullDocument03_Bad.xml", "Resources.FullDocument03_Good.xml")]
    [DataRow("Resources.FullDocument04_Bad.xml", "Resources.FullDocument04_Good.xml")]
    public void FormatXaml_FullDocument(string fileBad, string fileGood)
    {
        // Arrange.
        string input = TestHelper.ReadResource(fileBad);
        string expected = TestHelper.ReadResource(fileGood);

        // Replace tabs with spaces.
        expected = expected.Replace("\t", "    ");

        var sw = Stopwatch.StartNew();

        // Act.
        string actual = XamlFormatter.FormatXaml(input, -1, -1);

        sw.Stop();
        Debug.WriteLine($"FormatXaml_FullDocument: {sw.ElapsedMilliseconds}ms");

        TestHelper.WriteTestResultToFile(actual);

        // Assert.
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    public void SplitLinesTest()
    {
        string text = "First line\r\nSecond line\rThird line\nFourth line";
        string[] lines = XamlFormatter.SplitLines(text);
        Assert.AreEqual(4, lines.Length);
    }

    [TestMethod()]
    [DataRow("Resources.SingleLine01_Bad.xml", "Resources.SingleLine01_Good.xml")]
    [DataRow("Resources.SingleLine02_Bad.xml", "Resources.SingleLine02_Good.xml")]
    [DataRow("Resources.SingleLine03_Bad.xml", "Resources.SingleLine03_Good.xml")]
    [DataRow("Resources.SingleLine04_Bad.xml", "Resources.SingleLine04_Good.xml")]
    [DataRow("Resources.SingleLine05_Bad.xml", "Resources.SingleLine05_Good.xml")]
    [DataRow("Resources.SingleLineComment01_Bad.xml", "Resources.SingleLineComment01_Good.xml")]
    [DataRow("Resources.SingleLineComment02_Bad.xml", "Resources.SingleLineComment02_Good.xml")]
    public void RemoveExtraSpacesBetweenAttributesLineTest(string fileBad, string fileGood)
    {
        var input = TestHelper.ReadResource(fileBad);
        var expected = TestHelper.ReadResource(fileGood);

        var actual = XamlFormatter.RemoveExtraSpacesBetweenAttributesLine(input);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow("Value=\"Black\" />", "Value=\"Black\" />")]
    [DataRow("   Value=\"Black\" />", "   Value=\"Black\" />")]
    public void RemoveExtraSpacesBetweenAttributesLine_Manual(string input, string expected)
    {
        var actual = XamlFormatter.RemoveExtraSpacesBetweenAttributesLine(input);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow(" Value=\"Black\" />", " Value=\"Black\" />", -1)]
    [DataRow(" Value=\"Black\"/>", " Value=\"Black\"/>", -1)]
    [DataRow(" Value=\"Black\"          />", " Value=\"Black\" />", -1)]
    [DataRow(" Value=\"Black\" />", " Value=\"Black\"/>", 0)]
    [DataRow(" Value=\"Black\"/>", " Value=\"Black\"/>", 0)]
    [DataRow(" Value=\"Black\"          />", " Value=\"Black\"/>", 0)]
    [DataRow(" Value=\"Black\" />", " Value=\"Black\" />", 1)]
    [DataRow(" Value=\"Black\"/>", " Value=\"Black\" />", 1)]
    [DataRow(" Value=\"Black\"          />", " Value=\"Black\" />", 1)]
    public void RemoveExtraSpacesBetweenAttributesLine_ClosingTagSpaces(string input, string expected, int closingTagSpaces)
    {
        var actual = XamlFormatter.RemoveExtraSpacesBetweenAttributesLine(input, closingTagSpaces: closingTagSpaces);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow(" Value=\"Black\" >", " Value=\"Black\" >", -1)]
    [DataRow(" Value=\"Black\">", " Value=\"Black\">", -1)]
    [DataRow(" Value=\"Black\"          >", " Value=\"Black\" >", -1)]
    [DataRow(" Value=\"Black\" >", " Value=\"Black\">", 0)]
    [DataRow(" Value=\"Black\">", " Value=\"Black\">", 0)]
    [DataRow(" Value=\"Black\"          >", " Value=\"Black\">", 0)]
    [DataRow(" Value=\"Black\" >", " Value=\"Black\" >", 1)]
    [DataRow(" Value=\"Black\">", " Value=\"Black\" >", 1)]
    [DataRow(" Value=\"Black\"          >", " Value=\"Black\" >", 1)]
    public void RemoveExtraSpacesBetweenAttributesLine_EndTagSpaces(string input, string expected, int endingTagSpaces)
    {
        var actual = XamlFormatter.RemoveExtraSpacesBetweenAttributesLine(input, endingTagSpaces: endingTagSpaces);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow("Resources.SplitTagsPerLine01_Bad.xml", "Resources.SplitTagsPerLine01_Good.xml")]
    [DataRow("Resources.SplitTagsPerLine02_Bad.xml", "Resources.SplitTagsPerLine02_Good.xml")]
    public void SplitTagsPerLineTest(string fileBad, string fileGood)
    {
        // Arrange.
        var input = TestHelper.ReadResource(fileBad);
        var expected = TestHelper.ReadResource(fileGood);

        // Replace tabs with spaces.
        expected = expected.Replace("\t", "    ");

        // Act.
        var actual = XamlFormatter.SplitTagsPerLine(input);

        // Assert.
        TestHelper.WriteTestResultToFile(actual);
        Assert.AreEqual(expected, actual);
    }
}