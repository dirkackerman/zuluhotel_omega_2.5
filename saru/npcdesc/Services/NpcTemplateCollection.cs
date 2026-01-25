using NpcDesc.Models;

namespace NpcDesc.Services;

public class NpcTemplateCollection
{
    private readonly Dictionary<string, NpcTemplate> _templatesByName;
    
    public List<NpcTemplate> Templates { get; }
    public LootGroupCollection? LootGroups { get; private set; }

    public NpcTemplateCollection(List<NpcTemplate> templates)
    {
        Templates = templates;
        // Use last occurrence in case of duplicates (later definitions override earlier ones)
        _templatesByName = new Dictionary<string, NpcTemplate>(StringComparer.OrdinalIgnoreCase);
        foreach (var template in templates)
        {
            _templatesByName[template.TemplateName] = template;
        }
    }

    public NpcTemplate? GetByName(string templateName)
    {
        return _templatesByName.TryGetValue(templateName, out var template) ? template : null;
    }

    public IEnumerable<NpcTemplate> GetByType(string type)
    {
        return Templates.Where(t => 
            t.GetSlayerType()?.Equals(type, StringComparison.OrdinalIgnoreCase) == true);
    }

    public IEnumerable<NpcTemplate> GetByAlignment(string alignment)
    {
        return Templates.Where(t => 
            t.Alignment?.Equals(alignment, StringComparison.OrdinalIgnoreCase) == true);
    }

    public IEnumerable<NpcTemplate> GetBosses()
    {
        return Templates.Where(t => t.IsBoss);
    }

    public IEnumerable<NpcTemplate> GetByLootGroup(int lootGroupId)
    {
        return Templates.Where(t => t.LootGroupId == lootGroupId);
    }

    public void LinkLootGroups(LootGroupCollection lootGroups)
    {
        LootGroups = lootGroups;

        foreach (var template in Templates)
        {
            if (template.LootGroupId.HasValue)
            {
                template.LootGroup = lootGroups.GetLootGroup(template.LootGroupId.Value);
            }
        }
    }

    public IEnumerable<PossibleLootItem> GetPossibleLoot(string templateName)
    {
        var template = GetByName(templateName);
        if (template?.LootGroupId == null || LootGroups == null)
        {
            return Enumerable.Empty<PossibleLootItem>();
        }

        return LootGroups.GetPossibleItems(template.LootGroupId.Value);
    }

    public Dictionary<string, int> GetTypeDistribution()
    {
        return Templates
            .Where(t => t.GetSlayerType() != null)
            .GroupBy(t => t.GetSlayerType()!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public Dictionary<string, int> GetAlignmentDistribution()
    {
        return Templates
            .Where(t => t.Alignment != null)
            .GroupBy(t => t.Alignment!)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
