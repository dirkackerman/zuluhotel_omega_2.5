using Cocona;
using NpcDesc.Models;
using NpcDesc.Services;

namespace NpcDesc.Commands;

public class NpcDescCommands
{
    private static readonly string DefaultNpcDescPath = Path.Combine("..", "..", "..", "..", "..", "config", "npcdesc.cfg");
    private static readonly string DefaultLootGroupPath = Path.Combine("..", "..", "..", "..", "..", "config", "nlootgroup.cfg");

    [PrimaryCommand]
    [Command(Description = "Interactive mode to browse NPC templates and view statistics")]
    public int Interactive(
        [Option('n', Description = "Path to npcdesc.cfg file")] string? npcDescPath = null,
        [Option('l', Description = "Path to nlootgroup.cfg file")] string? lootGroupPath = null)
    {
        npcDescPath ??= DefaultNpcDescPath;
        lootGroupPath ??= DefaultLootGroupPath;

        if (!File.Exists(npcDescPath))
        {
            Console.WriteLine($"Error: NPC description file not found: {npcDescPath}");
            return 1;
        }

        if (!File.Exists(lootGroupPath))
        {
            Console.WriteLine($"Error: Loot group file not found: {lootGroupPath}");
            return 1;
        }

        Console.WriteLine("NpcDesc Parser");
        Console.WriteLine("==============");
        Console.WriteLine();

        var loader = new GameConfigLoader();
        var collection = loader.LoadAll(npcDescPath, lootGroupPath);

        DisplaySummary(collection);

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
    }

    [Command("export-csv", Description = "Export NPC templates to a CSV file")]
    public int ExportCsv(
        [Option('o', Description = "Output CSV file path")] string output,
        [Option('n', Description = "Path to npcdesc.cfg file")] string? npcDescPath = null,
        [Option('l', Description = "Path to nlootgroup.cfg file")] string? lootGroupPath = null)
    {
        npcDescPath ??= DefaultNpcDescPath;
        lootGroupPath ??= DefaultLootGroupPath;

        if (!File.Exists(npcDescPath))
        {
            Console.WriteLine($"Error: NPC description file not found: {npcDescPath}");
            return 1;
        }

        if (!File.Exists(lootGroupPath))
        {
            Console.WriteLine($"Error: Loot group file not found: {lootGroupPath}");
            return 1;
        }

        Console.WriteLine($"Loading configuration files...");
        var loader = new GameConfigLoader();
        var collection = loader.LoadAll(npcDescPath, lootGroupPath);

        Console.WriteLine($"Exporting {collection.Templates.Count} templates to CSV...");

        using var writer = new StreamWriter(output);
        
        // Write header
        writer.WriteLine("Name,MagicItemLevel,MagicItemChance,LootGroupId");

        // Write data rows
        foreach (var template in collection.Templates.OrderBy(t => t.TemplateName))
        {
            var name = EscapeCsvField(template.TemplateName);
            var magicItemLevel = template.MagicItemLevel?.ToString() ?? "";
            var magicItemChance = template.MagicItemChance?.ToString() ?? "";
            var lootGroupId = template.LootGroupId?.ToString() ?? "";

            writer.WriteLine($"{name},{magicItemLevel},{magicItemChance},{lootGroupId}");
        }

        Console.WriteLine($"Successfully exported to: {Path.GetFullPath(output)}");
        return 0;
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return "";
        }

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private static void DisplaySummary(NpcTemplateCollection collection)
    {
        Console.WriteLine("Summary Statistics");
        Console.WriteLine("------------------");
        Console.WriteLine($"Total NPC templates parsed: {collection.Templates.Count}");
        Console.WriteLine($"Total loot groups parsed: {collection.LootGroups?.LootGroups.Count ?? 0}");
        Console.WriteLine($"Total item groups parsed: {collection.LootGroups?.ItemGroups.Count ?? 0}");
        Console.WriteLine();

        Console.WriteLine("Templates by Alignment:");
        foreach (var (alignment, count) in collection.GetAlignmentDistribution().OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {alignment}: {count}");
        }
        Console.WriteLine();

        Console.WriteLine("Templates by Slayer Type:");
        foreach (var (type, count) in collection.GetTypeDistribution().OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {type}: {count}");
        }
        Console.WriteLine();

        var bosses = collection.GetBosses().ToList();
        Console.WriteLine($"Total Bosses: {bosses.Count}");
        Console.WriteLine();
    }

    private static void DisplayTemplate(NpcTemplate template, NpcTemplateCollection collection)
    {
        Console.WriteLine();
        Console.WriteLine($"NPC Template: {template.TemplateName}");
        Console.WriteLine($"  Name             : {template.Name}");
        Console.WriteLine($"  Script           : {template.Script ?? "N/A"}");
        Console.WriteLine($"  Type             : {template.GetSlayerType() ?? "N/A"}");
        Console.WriteLine($"  ObjType          : 0x{template.ObjType:X} ({template.ObjType})");
        Console.WriteLine($"  Color            : {template.Color}, TrueColor: {template.TrueColor}");
        Console.WriteLine($"  Magic Item Chance: {template.MagicItemChance}");
        Console.WriteLine($"  Magic Item Level : {template.MagicItemLevel}");
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

            var lootItems = collection.GetPossibleLoot(template.TemplateName).ToList();
            if (lootItems.Count > 0)
            {
                Console.WriteLine($"  Possible Loot ({lootItems.Count} item(s)):");
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
}
