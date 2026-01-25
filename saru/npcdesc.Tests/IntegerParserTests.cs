using NpcDesc.Utilities;

namespace NpcDesc.Tests;

public class IntegerParserTests
{
    [Theory]
    [InlineData("123", 123)]
    [InlineData("0", 0)]
    [InlineData("-500", -500)]
    [InlineData("0x190", 400)]
    [InlineData("0xc8", 200)]
    [InlineData("0xFF", 255)]
    [InlineData("0x0", 0)]
    public void Parse_ValidValues_ReturnsCorrectInteger(string input, int expected)
    {
        var result = IntegerParser.Parse(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true, 123)]
    [InlineData("0x190", true, 400)]
    [InlineData("invalid", false, 0)]
    [InlineData("", false, 0)]
    public void TryParse_ReturnsExpectedResult(string input, bool expectedSuccess, int expectedValue)
    {
        var success = IntegerParser.TryParse(input, out var result);
        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.Equal(expectedValue, result);
        }
    }
}
