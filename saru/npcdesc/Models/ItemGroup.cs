namespace NpcDesc.Models;

public class ItemGroup
{
    public string Name { get; set; } = string.Empty;
    public List<LootEntry> Entries { get; set; } = new();
}
