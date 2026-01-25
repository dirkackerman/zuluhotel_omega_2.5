using NpcDesc.Models;
using NpcDesc.Services;

namespace NpcDesc.Tests;

public class LootGroupParserTests
{
    private readonly LootGroupParser _parser = new();

    [Fact]
    public void ParseContent_SimpleLootGroup_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Stack       10d5        GoldCoin
    Item        1           Sword
    Random      1d2         NormalArmor     50
}";

        var collection = _parser.ParseContent(content);

        Assert.Single(collection.LootGroups);
        var group = collection.LootGroups[1];
        Assert.Equal(1, group.Id);
        Assert.Equal(3, group.Entries.Count);
    }

    [Fact]
    public void ParseContent_StackEntry_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Stack       10d5        GoldCoin
}";

        var collection = _parser.ParseContent(content);
        var entry = collection.LootGroups[1].Entries[0] as ItemLootEntry;

        Assert.NotNull(entry);
        Assert.Equal(LootEntryType.Stack, entry.EntryType);
        Assert.Equal("GoldCoin", entry.ItemName);
        Assert.Equal(10, entry.Amount.NumDice);
        Assert.Equal(5, entry.Amount.DiceSides);
        Assert.True(entry.IsStack);
        Assert.Equal(100, entry.ChancePercent);
    }

    [Fact]
    public void ParseContent_ItemEntry_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Item        1           Sword           75
}";

        var collection = _parser.ParseContent(content);
        var entry = collection.LootGroups[1].Entries[0] as ItemLootEntry;

        Assert.NotNull(entry);
        Assert.Equal(LootEntryType.Item, entry.EntryType);
        Assert.Equal("Sword", entry.ItemName);
        Assert.Equal(1, entry.Amount.NumDice);
        Assert.False(entry.IsStack);
        Assert.Equal(75, entry.ChancePercent);
    }

    [Fact]
    public void ParseContent_RandomEntry_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Random      1d2         NormalArmor     30
}";

        var collection = _parser.ParseContent(content);
        var entry = collection.LootGroups[1].Entries[0] as RandomGroupEntry;

        Assert.NotNull(entry);
        Assert.Equal(LootEntryType.Random, entry.EntryType);
        Assert.Equal("NormalArmor", entry.GroupName);
        Assert.Equal(1, entry.Amount.NumDice);
        Assert.Equal(2, entry.Amount.DiceSides);
        Assert.Equal(30, entry.ChancePercent);
    }

    [Fact]
    public void ParseContent_UniqueEntry_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Unique      RareItem
}";

        var collection = _parser.ParseContent(content);
        var entry = collection.LootGroups[1].Entries[0] as ItemLootEntry;

        Assert.NotNull(entry);
        Assert.Equal(LootEntryType.Unique, entry.EntryType);
        Assert.Equal("RareItem", entry.ItemName);
        Assert.True(entry.IsUnique);
    }

    [Fact]
    public void ParseContent_ItemGroup_ParsesCorrectly()
    {
        var content = @"
Group Reagents
{
    Stack   2d3     Garlic
    Stack   2d3     SpiderSilk
    Stack   2d3     MandrakeRoot
}";

        var collection = _parser.ParseContent(content);

        Assert.Single(collection.ItemGroups);
        var group = collection.ItemGroups["Reagents"];
        Assert.Equal("Reagents", group.Name);
        Assert.Equal(3, group.Entries.Count);
    }

    [Fact]
    public void ParseContent_MultipleLootGroups_ParsesAll()
    {
        var content = @"
lootgroup 1
{
    Stack       10d5        GoldCoin
}

lootgroup 2
{
    Item        1           Club
}

lootgroup 17
{
    Random      1           NormalArmor
}";

        var collection = _parser.ParseContent(content);

        Assert.Equal(3, collection.LootGroups.Count);
        Assert.True(collection.LootGroups.ContainsKey(1));
        Assert.True(collection.LootGroups.ContainsKey(2));
        Assert.True(collection.LootGroups.ContainsKey(17));
    }

    [Fact]
    public void ParseContent_CommentsIgnored_ParsesCorrectly()
    {
        var content = @"
// This is a comment
//kobold1, orc1, ratman3
lootgroup 1
{
    Stack       10d5        GoldCoin    // inline comment
}";

        var collection = _parser.ParseContent(content);

        Assert.Single(collection.LootGroups);
        var entry = collection.LootGroups[1].Entries[0] as ItemLootEntry;
        Assert.Equal("GoldCoin", entry?.ItemName);
    }

    [Fact]
    public void ParseContent_ResolvesGroupReferences()
    {
        var content = @"
Group Reagents
{
    Stack   2d3     Garlic
}

lootgroup 1
{
    Random      1d2         Reagents        50
}";

        var collection = _parser.ParseContent(content);

        var randomEntry = collection.LootGroups[1].Entries[0] as RandomGroupEntry;
        Assert.NotNull(randomEntry?.ResolvedGroup);
        Assert.Equal("Reagents", randomEntry.ResolvedGroup.Name);
    }

    [Fact]
    public void GetPossibleItems_ReturnsItems()
    {
        var content = @"
Group Reagents
{
    Stack   2d3     Garlic
    Stack   2d3     SpiderSilk
}

lootgroup 1
{
    Stack       10d5        GoldCoin
    Random      1d2         Reagents        50
}";

        var collection = _parser.ParseContent(content);
        var possibleItems = collection.GetPossibleItems(1).ToList();

        Assert.True(possibleItems.Count >= 3);
        Assert.Contains(possibleItems, i => i.ItemName == "GoldCoin");
        Assert.Contains(possibleItems, i => i.ItemName == "Garlic");
        Assert.Contains(possibleItems, i => i.ItemName == "SpiderSilk");
    }

    [Fact]
    public void ParseContent_DiceNotationWithBonus_ParsesCorrectly()
    {
        var content = @"
lootgroup 1
{
    Stack       1d300+250   GoldCoin
}";

        var collection = _parser.ParseContent(content);
        var entry = collection.LootGroups[1].Entries[0] as ItemLootEntry;

        Assert.NotNull(entry);
        Assert.Equal(1, entry.Amount.NumDice);
        Assert.Equal(300, entry.Amount.DiceSides);
        Assert.Equal(250, entry.Amount.Bonus);
        Assert.Equal(251, entry.Amount.MinValue);
        Assert.Equal(550, entry.Amount.MaxValue);
    }
}
