namespace NpcDesc.Models;

public class LootGroup
{
    public int Id { get; set; }
    public List<LootEntry> Entries { get; set; } = new();
}
