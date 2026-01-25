using NpcDesc.Services;

namespace NpcDesc.Tests;

public class IntegrationTests
{
    private static readonly string ConfigPath = Path.Combine("..", "..", "..", "..", "..", "config");
    private static readonly string NpcDescPath = Path.Combine(ConfigPath, "npcdesc.cfg");
    private static readonly string LootGroupPath = Path.Combine(ConfigPath, "nlootgroup.cfg");

    private static bool ConfigFilesExist => File.Exists(NpcDescPath) && File.Exists(LootGroupPath);

    [SkippableFact]
    public void ParseNpcDescFile_ParsesWithoutErrors()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var parser = new NpcDescParser();
        var templates = parser.Parse(NpcDescPath);

        Assert.NotEmpty(templates);
        Assert.True(templates.Count > 100, $"Expected many templates, got {templates.Count}");
    }

    [SkippableFact]
    public void ParseNpcDescFile_ContainsExpectedTemplates()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var parser = new NpcDescParser();
        var templates = parser.Parse(NpcDescPath);
        var collection = new NpcTemplateCollection(templates);

        // Check for known templates from the file
        var troll1 = collection.GetByName("troll1");
        Assert.NotNull(troll1);
        Assert.Equal("a Troll", troll1.Name);
        Assert.Equal(175, troll1.Str);
        Assert.Equal(25, troll1.Int);
        Assert.Equal(60, troll1.Dex);
        Assert.Equal("evil", troll1.Alignment);
        Assert.True(troll1.Hostile);
        Assert.Equal(17, troll1.LootGroupId);

        var beckon = collection.GetByName("beckon");
        Assert.NotNull(beckon);
        Assert.Equal("a fairy", beckon.Name);
        Assert.Equal(200, beckon.Str);
        Assert.Equal("good", beckon.Alignment);
    }

    [SkippableFact]
    public void ParseNpcDescFile_ParsesSkillsCorrectly()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var parser = new NpcDescParser();
        var templates = parser.Parse(NpcDescPath);
        var collection = new NpcTemplateCollection(templates);

        var beckon = collection.GetByName("beckon");
        Assert.NotNull(beckon);
        Assert.Equal(120, beckon.Skills["Parry"]);
        Assert.Equal(100, beckon.Skills["Tactics"]);
        Assert.Equal(120, beckon.Skills["Magery"]);
    }

    [SkippableFact]
    public void ParseNpcDescFile_ParsesSpellsCorrectly()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var parser = new NpcDescParser();
        var templates = parser.Parse(NpcDescPath);
        var collection = new NpcTemplateCollection(templates);

        var beckon = collection.GetByName("beckon");
        Assert.NotNull(beckon);
        Assert.Contains("ebolt", beckon.Spells);
        Assert.Contains("flamestrike", beckon.Spells);
        Assert.Contains("lightning", beckon.Spells);
    }

    [SkippableFact]
    public void ParseNpcDescFile_ParsesCPropsCorrectly()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var parser = new NpcDescParser();
        var templates = parser.Parse(NpcDescPath);
        var collection = new NpcTemplateCollection(templates);

        var beckon = collection.GetByName("beckon");
        Assert.NotNull(beckon);
        Assert.Equal("Human", beckon.CustomProperties["Type"].StringValue);
        Assert.Equal(100, beckon.CustomProperties["BaseStrmod"].IntValue);
        Assert.Equal(300, beckon.CustomProperties["BaseIntmod"].IntValue);
    }

    [SkippableFact]
    public void ParseLootGroupFile_ParsesWithoutErrors()
    {
        Skip.IfNot(File.Exists(LootGroupPath), $"Config file not found: {LootGroupPath}");

        var parser = new LootGroupParser();
        var collection = parser.Parse(LootGroupPath);

        Assert.NotEmpty(collection.LootGroups);
        Assert.NotEmpty(collection.ItemGroups);
    }

    [SkippableFact]
    public void ParseLootGroupFile_ContainsExpectedGroups()
    {
        Skip.IfNot(File.Exists(LootGroupPath), $"Config file not found: {LootGroupPath}");

        var parser = new LootGroupParser();
        var collection = parser.Parse(LootGroupPath);

        // Check for known loot groups
        Assert.True(collection.LootGroups.ContainsKey(1));
        Assert.True(collection.LootGroups.ContainsKey(17));

        // Check for known item groups
        Assert.True(collection.ItemGroups.ContainsKey("Reagents"));
        Assert.True(collection.ItemGroups.ContainsKey("NormalWeapons"));
        Assert.True(collection.ItemGroups.ContainsKey("Gems"));
    }

    [SkippableFact]
    public void ParseLootGroupFile_LootGroup1_HasExpectedEntries()
    {
        Skip.IfNot(File.Exists(LootGroupPath), $"Config file not found: {LootGroupPath}");

        var parser = new LootGroupParser();
        var collection = parser.Parse(LootGroupPath);

        var group1 = collection.GetLootGroup(1);
        Assert.NotNull(group1);
        Assert.True(group1.Entries.Count >= 4);
    }

    [SkippableFact]
    public void ParseLootGroupFile_ReagentsGroup_HasExpectedItems()
    {
        Skip.IfNot(File.Exists(LootGroupPath), $"Config file not found: {LootGroupPath}");

        var parser = new LootGroupParser();
        var collection = parser.Parse(LootGroupPath);

        var reagents = collection.GetItemGroup("Reagents");
        Assert.NotNull(reagents);
        Assert.True(reagents.Entries.Count >= 8);
    }

    [SkippableFact]
    public void FullIntegration_LoadsAndLinksCorrectly()
    {
        Skip.IfNot(ConfigFilesExist, "Config files not found");

        var loader = new GameConfigLoader();
        var collection = loader.LoadAll(NpcDescPath, LootGroupPath);

        // Verify NPC templates loaded
        Assert.NotEmpty(collection.Templates);

        // Verify loot groups loaded
        Assert.NotNull(collection.LootGroups);
        Assert.NotEmpty(collection.LootGroups.LootGroups);

        // Verify linking works
        var troll1 = collection.GetByName("troll1");
        Assert.NotNull(troll1);
        Assert.NotNull(troll1.LootGroup);
        Assert.Equal(17, troll1.LootGroup.Id);

        // Verify we can get possible loot
        var possibleLoot = collection.GetPossibleLoot("troll1").ToList();
        Assert.NotEmpty(possibleLoot);
    }

    [SkippableFact]
    public void FullIntegration_GetTypeDistribution_ReturnsResults()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var loader = new GameConfigLoader();
        var collection = loader.LoadNpcOnly(NpcDescPath);

        var typeDistribution = collection.GetTypeDistribution();
        Assert.NotEmpty(typeDistribution);
        Assert.True(typeDistribution.ContainsKey("Human") || typeDistribution.ContainsKey("Undead") || typeDistribution.ContainsKey("Daemon"));
    }

    [SkippableFact]
    public void FullIntegration_GetBosses_ReturnsBosses()
    {
        Skip.IfNot(File.Exists(NpcDescPath), $"Config file not found: {NpcDescPath}");

        var loader = new GameConfigLoader();
        var collection = loader.LoadNpcOnly(NpcDescPath);

        var bosses = collection.GetBosses().ToList();
        Assert.NotEmpty(bosses);
        Assert.All(bosses, b => Assert.True(b.IsBoss));
    }
}
