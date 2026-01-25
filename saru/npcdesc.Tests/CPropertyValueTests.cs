using NpcDesc.Models;

namespace NpcDesc.Tests;

public class CPropertyValueTests
{
    [Theory]
    [InlineData("i100", CPropertyType.Integer, 100, null)]
    [InlineData("i-50", CPropertyType.Integer, -50, null)]
    [InlineData("i0", CPropertyType.Integer, 0, null)]
    [InlineData("sHuman", CPropertyType.String, null, "Human")]
    [InlineData("sFastest", CPropertyType.String, null, "Fastest")]
    [InlineData("sElemental", CPropertyType.String, null, "Elemental")]
    public void FromTypedString_ParsesCorrectly(string input, CPropertyType expectedType, int? expectedInt, string? expectedString)
    {
        var result = CPropertyValue.FromTypedString(input);

        Assert.Equal(expectedType, result.Type);
        Assert.Equal(expectedInt, result.IntValue);
        Assert.Equal(expectedString, result.StringValue);
    }

    [Fact]
    public void ToString_Integer_ReturnsCorrectFormat()
    {
        var value = CPropertyValue.FromTypedString("i100");
        Assert.Equal("i100", value.ToString());
    }

    [Fact]
    public void ToString_String_ReturnsCorrectFormat()
    {
        var value = CPropertyValue.FromTypedString("sHuman");
        Assert.Equal("sHuman", value.ToString());
    }
}
