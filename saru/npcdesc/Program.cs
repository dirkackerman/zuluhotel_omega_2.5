using NpcDesc.Models;
using NpcDesc.Services;

var defaultNpcDescPath = Path.Combine("..", "..", "..", "..", "..", "config", "npcdesc.cfg");
var defaultLootGroupPath = Path.Combine("..", "..", "..", "..", "..", "config", "nlootgroup.cfg");

var npcDescPath = args.Length > 0 ? args[0] : defaultNpcDescPath;
var lootGroupPath = args.Length > 1 ? args[1] : defaultLootGroupPath;

if (!File.Exists(npcDescPath))
{
    Console.WriteLine($"Error: NPC description file not found: {npcDescPath} / { Path.GetFullPath(npcDescPath) }");
    return 1;
}

if (!File.Exists(lootGroupPath))
{
    Console.WriteLine($"Error: Loot group file not found: {lootGroupPath} / { Path.GetFullPath(lootGroupPath) }");
    return 1;
}

Console.WriteLine("NpcDesc Parser");
Console.WriteLine("==============");
Console.WriteLine();

var loader = new GameConfigLoader();
var collection = loader.LoadAll(npcDescPath, lootGroupPath);

// Display summary statistics
Console.WriteLine("Summary Statistics");
Console.WriteLine("------------------");
Console.WriteLine($"Total NPC templates parsed: {collection.Templates.Count}");
Console.WriteLine($"Total loot groups parsed: {collection.LootGroups?.LootGroups.Count ?? 0}");
Console.WriteLine($"Total item groups parsed: {collection.LootGroups?.ItemGroups.Count ?? 0}");
Console.WriteLine();

// Templates by alignment
Console.WriteLine("Templates by Alignment:");
foreach (var (alignment, count) in collection.GetAlignmentDistribution().OrderByDescending(x => x.Value))
{
    Console.WriteLine($"  {alignment}: {count}");
}
Console.WriteLine();

// Templates by slayer type
Console.WriteLine("Templates by Slayer Type:");
foreach (var (type, count) in collection.GetTypeDistribution().OrderByDescending(x => x.Value))
{
    Console.WriteLine($"  {type}: {count}");
}
Console.WriteLine();

// Boss counts
var bosses = collection.GetBosses().ToList();
Console.WriteLine($"Total Bosses: {bosses.Count}");
Console.WriteLine();

// Interactive query mode
Console.WriteLine("Enter an NPC template name to view details (or 'quit' to exit):");
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrEmpty(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var template = collection.GetByName(input);
    if (template == null)
    {
        Console.WriteLine($"Template '{input}' not found.");
        Console.WriteLine();
        continue;
    }

    DisplayTemplate(template, collection);
    Console.WriteLine();
}

return 0;

static void DisplayTemplate(NpcTemplate template, NpcTemplateCollection collection)
{
    Console.WriteLine();
    Console.WriteLine($"NPC Template: {template.TemplateName}");
    Console.WriteLine($"  Name: {template.Name}");
    Console.WriteLine($"  Script: {template.Script ?? "N/A"}");
    Console.WriteLine($"  Type: {template.GetSlayerType() ?? "N/A"}");
    Console.WriteLine($"  ObjType: 0x{template.ObjType:X} ({template.ObjType})");
    Console.WriteLine($"  Color: {template.Color}, TrueColor: {template.TrueColor}");
    Console.WriteLine();
    Console.WriteLine($"  Stats: STR {template.Str}, INT {template.Int}, DEX {template.Dex}");
    if (template.Hits.HasValue || template.Mana.HasValue || template.Stam.HasValue)
    {
        Console.WriteLine($"  Resources: HITS {template.Hits ?? 0}, MANA {template.Mana ?? 0}, STAM {template.Stam ?? 0}");
    }
    Console.WriteLine();
    Console.WriteLine($"  Alignment: {template.Alignment ?? "N/A"}");
    Console.WriteLine($"  Hostile: {template.Hostile}");
    Console.WriteLine($"  Is Boss: {template.IsBoss}");
    
    if (template.Skills.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("  Skills:");
        foreach (var (skill, value) in template.Skills.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"    {skill}: {value}");
        }
    }

    if (template.Spells.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"  Spells: {string.Join(", ", template.Spells)}");
    }

    if (template.Equipment.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"  Equipment: {string.Join(", ", template.Equipment)}");
    }

    if (template.CustomProperties.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("  Custom Properties:");
        foreach (var (name, value) in template.CustomProperties.OrderBy(x => x.Key))
        {
            Console.WriteLine($"    {name}: {value}");
        }
    }

    if (template.LootGroupId.HasValue)
    {
        Console.WriteLine();
        Console.WriteLine($"  Loot Group: {template.LootGroupId}");
        
        var lootItems = collection.GetPossibleLoot(template.TemplateName).Take(20).ToList();
        if (lootItems.Count > 0)
        {
            Console.WriteLine("  Possible Loot (first 20):");
            foreach (var item in lootItems)
            {
                var amountStr = item.Amount.DiceSides == 1 
                    ? item.Amount.MinValue.ToString() 
                    : $"{item.Amount} ({item.Amount.MinValue}-{item.Amount.MaxValue})";
                var stackStr = item.IsStack ? " [Stack]" : "";
                Console.WriteLine($"    {item.ItemName} x{amountStr} ({item.EffectiveChance:F1}%){stackStr}");
            }
        }
    }
}
