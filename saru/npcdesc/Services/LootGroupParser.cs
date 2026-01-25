using NpcDesc.Models;
using NpcDesc.Utilities;

namespace NpcDesc.Services;

public class LootGroupParser
{
    public LootGroupCollection Parse(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return ParseContent(content);
    }

    public LootGroupCollection Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        return ParseContent(content);
    }

    public LootGroupCollection ParseContent(string content)
    {
        var collection = new LootGroupCollection();
        var lines = content.Split('\n');
        var lineIndex = 0;

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }

            // Look for lootgroup or Group declaration
            if (line.StartsWith("lootgroup", StringComparison.OrdinalIgnoreCase))
            {
                var lootGroup = ParseLootGroup(lines, ref lineIndex);
                if (lootGroup != null)
                {
                    collection.LootGroups[lootGroup.Id] = lootGroup;
                }
            }
            else if (line.StartsWith("Group", StringComparison.OrdinalIgnoreCase) && 
                     !line.StartsWith("Group\t", StringComparison.OrdinalIgnoreCase) &&
                     !line.StartsWith("Group ", StringComparison.OrdinalIgnoreCase) ||
                     (line.StartsWith("Group ", StringComparison.OrdinalIgnoreCase) && 
                      lines.Skip(lineIndex + 1).TakeWhile(l => !l.Trim().StartsWith("lootgroup", StringComparison.OrdinalIgnoreCase))
                           .Any(l => l.Trim() == "{")))
            {
                var itemGroup = ParseItemGroup(lines, ref lineIndex);
                if (itemGroup != null)
                {
                    collection.ItemGroups[itemGroup.Name] = itemGroup;
                }
            }
            else
            {
                lineIndex++;
            }
        }

        collection.ResolveReferences();
        return collection;
    }

    private static bool IsComment(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("//");
    }

    private static LootGroup? ParseLootGroup(string[] lines, ref int lineIndex)
    {
        var headerLine = lines[lineIndex].Trim();

        // Parse loot group ID: "lootgroup <id>"
        var parts = headerLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[1], out var groupId))
        {
            lineIndex++;
            return null;
        }

        var lootGroup = new LootGroup { Id = groupId };
        lineIndex++;

        // Find opening brace
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }
            if (line == "{")
            {
                lineIndex++;
                break;
            }
            break;
        }

        // Parse entries until closing brace
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();

            if (line == "}")
            {
                lineIndex++;
                break;
            }

            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }

            var entry = ParseLootEntry(line);
            if (entry != null)
            {
                lootGroup.Entries.Add(entry);
            }
            lineIndex++;
        }

        return lootGroup;
    }

    private static ItemGroup? ParseItemGroup(string[] lines, ref int lineIndex)
    {
        var headerLine = lines[lineIndex].Trim();

        // Parse group name: "Group <name>"
        var parts = headerLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            lineIndex++;
            return null;
        }

        var groupName = parts[1];
        var itemGroup = new ItemGroup { Name = groupName };
        lineIndex++;

        // Find opening brace
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }
            if (line == "{")
            {
                lineIndex++;
                break;
            }
            break;
        }

        // Parse entries until closing brace
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();

            if (line == "}")
            {
                lineIndex++;
                break;
            }

            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }

            var entry = ParseLootEntry(line);
            if (entry != null)
            {
                itemGroup.Entries.Add(entry);
            }
            lineIndex++;
        }

        return itemGroup;
    }

    private static LootEntry? ParseLootEntry(string line)
    {
        // Remove inline comments
        var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
        if (commentIndex >= 0)
        {
            line = line[..commentIndex];
        }

        line = line.Trim();
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return null;
        }

        var entryType = tokens[0].ToLowerInvariant();

        return entryType switch
        {
            "stack" => ParseItemEntry(tokens, LootEntryType.Stack, isStack: true),
            "item" or "items" => ParseItemEntry(tokens, LootEntryType.Item, isStack: false),
            "unique" => ParseUniqueEntry(tokens),
            "random" => ParseRandomEntry(tokens),
            "group" => ParseGroupEntry(tokens),
            _ => null
        };
    }

    private static ItemLootEntry? ParseItemEntry(string[] tokens, LootEntryType type, bool isStack)
    {
        // Format: Stack/Item <amount> <itemName> [chance] [color]
        // Or:     Stack/Item <itemName> [chance] [color] (amount defaults to 1)
        if (tokens.Length < 2)
        {
            return null;
        }

        DiceRoll amount;
        string itemName;
        var chanceIndex = 3;

        // Check if second token is a dice notation or item name
        if (DiceRoll.TryParse(tokens[1], out var parsedAmount) && tokens.Length >= 3)
        {
            // Format with amount: Item 1d2 ItemName [chance]
            amount = parsedAmount;
            itemName = tokens[2];
            chanceIndex = 3;
        }
        else
        {
            // Format without amount: Item ItemName [chance]
            amount = DiceRoll.One;
            itemName = tokens[1];
            chanceIndex = 2;
        }

        var chance = 100;
        int? color = null;

        if (tokens.Length > chanceIndex && int.TryParse(tokens[chanceIndex], out var parsedChance))
        {
            chance = parsedChance;
        }

        if (tokens.Length > chanceIndex + 1 && IntegerParser.TryParse(tokens[chanceIndex + 1], out var parsedColor))
        {
            color = parsedColor;
        }

        return new ItemLootEntry
        {
            EntryType = type,
            Amount = amount,
            ItemName = itemName,
            ChancePercent = chance,
            Color = color,
            IsStack = isStack,
            IsUnique = false
        };
    }

    private static ItemLootEntry? ParseUniqueEntry(string[] tokens)
    {
        // Format: Unique <itemName>
        if (tokens.Length < 2)
        {
            return null;
        }

        return new ItemLootEntry
        {
            EntryType = LootEntryType.Unique,
            Amount = DiceRoll.One,
            ItemName = tokens[1],
            ChancePercent = 100,
            IsStack = false,
            IsUnique = true
        };
    }

    private static RandomGroupEntry? ParseRandomEntry(string[] tokens)
    {
        // Format: Random <amount> <groupName> [chance]
        if (tokens.Length < 3)
        {
            return null;
        }

        var amount = DiceRoll.Parse(tokens[1]);
        var groupName = tokens[2];
        var chance = 100;

        if (tokens.Length >= 4 && int.TryParse(tokens[3], out var parsedChance))
        {
            chance = parsedChance;
        }

        return new RandomGroupEntry
        {
            EntryType = LootEntryType.Random,
            Amount = amount,
            GroupName = groupName,
            ChancePercent = chance
        };
    }

    private static IncludeGroupEntry? ParseGroupEntry(string[] tokens)
    {
        // Format: Group <groupName>
        if (tokens.Length < 2)
        {
            return null;
        }

        return new IncludeGroupEntry
        {
            EntryType = LootEntryType.Group,
            Amount = DiceRoll.One,
            GroupName = tokens[1],
            ChancePercent = 100
        };
    }
}
