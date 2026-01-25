using NpcDesc.Utilities;

namespace NpcDesc.Tests;

public class DiceRollTests
{
    [Theory]
    [InlineData("5", 5, 1, 0)]
    [InlineData("10", 10, 1, 0)]
    [InlineData("2d6", 2, 6, 0)]
    [InlineData("1d100", 1, 100, 0)]
    [InlineData("10d5", 10, 5, 0)]
    [InlineData("2d6+3", 2, 6, 3)]
    [InlineData("1d100+50", 1, 100, 50)]
    [InlineData("1d300+250", 1, 300, 250)]
    public void Parse_ValidNotation_ReturnsCorrectValues(string notation, int expectedDice, int expectedSides, int expectedBonus)
    {
        var result = DiceRoll.Parse(notation);

        Assert.Equal(expectedDice, result.NumDice);
        Assert.Equal(expectedSides, result.DiceSides);
        Assert.Equal(expectedBonus, result.Bonus);
    }

    [Theory]
    [InlineData("5", 5, 5)]
    [InlineData("2d6", 2, 12)]
    [InlineData("1d100", 1, 100)]
    [InlineData("10d5", 10, 50)]
    [InlineData("2d6+3", 5, 15)]
    [InlineData("1d300+250", 251, 550)]
    public void MinMax_ReturnsCorrectRange(string notation, int expectedMin, int expectedMax)
    {
        var result = DiceRoll.Parse(notation);

        Assert.Equal(expectedMin, result.MinValue);
        Assert.Equal(expectedMax, result.MaxValue);
    }

    [Fact]
    public void Roll_ReturnsValueInRange()
    {
        var dice = DiceRoll.Parse("2d6+3");
        var rng = new Random(42); // Fixed seed for reproducibility

        for (var i = 0; i < 100; i++)
        {
            var result = dice.Roll(rng);
            Assert.InRange(result, dice.MinValue, dice.MaxValue);
        }
    }

    [Theory]
    [InlineData("5", "5")]
    [InlineData("2d6", "2d6")]
    [InlineData("2d6+3", "2d6+3")]
    public void ToString_ReturnsExpectedFormat(string notation, string expected)
    {
        var dice = DiceRoll.Parse(notation);
        Assert.Equal(expected, dice.ToString());
    }

    [Fact]
    public void TryParse_InvalidNotation_ReturnsFalse()
    {
        var result = DiceRoll.TryParse("invalid", out var dice);
        Assert.False(result);
    }

    [Fact]
    public void TryParse_ValidNotation_ReturnsTrue()
    {
        var result = DiceRoll.TryParse("2d6", out var dice);
        Assert.True(result);
        Assert.Equal(2, dice.NumDice);
        Assert.Equal(6, dice.DiceSides);
    }
}
