namespace NpcDesc.Services;

public class GameConfigLoader
{
    private readonly NpcDescParser _npcParser;
    private readonly LootGroupParser _lootParser;

    public GameConfigLoader()
    {
        _npcParser = new NpcDescParser();
        _lootParser = new LootGroupParser();
    }

    public NpcTemplateCollection LoadAll(string npcDescPath, string lootGroupPath)
    {
        var templates = _npcParser.Parse(npcDescPath);
        var lootGroups = _lootParser.Parse(lootGroupPath);

        var collection = new NpcTemplateCollection(templates);
        collection.LinkLootGroups(lootGroups);

        return collection;
    }

    public NpcTemplateCollection LoadNpcOnly(string npcDescPath)
    {
        var templates = _npcParser.Parse(npcDescPath);
        return new NpcTemplateCollection(templates);
    }

    public LootGroupCollection LoadLootGroupsOnly(string lootGroupPath)
    {
        return _lootParser.Parse(lootGroupPath);
    }
}
