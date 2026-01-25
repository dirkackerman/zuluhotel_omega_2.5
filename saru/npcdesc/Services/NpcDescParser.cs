using NpcDesc.Models;
using NpcDesc.Utilities;

namespace NpcDesc.Services;

public class NpcDescParser
{
    private static readonly HashSet<string> SkillNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Parry", "Tactics", "Swordsmanship", "Magery", "MaceFighting", "Fencing",
        "Archery", "Wrestling", "Healing", "Anatomy", "EvaluatingIntelligence",
        "EvaluateIntelligence", "DetectingHidden", "DetectHidden", "Hiding", "Stealth",
        "Poisoning", "SpiritSpeak", "SpiritSpeaking", "Meditation", "MagicResistance",
        "Alchemy", "Blacksmithy", "Tailoring", "Carpentry", "Tinkering", "Mining",
        "Lumberjacking", "Fishing", "Cooking", "Inscription", "Cartography",
        "Lockpicking", "RemoveTrap", "Tracking", "Veterinary", "AnimalLore",
        "AnimalTaming", "Herding", "Begging", "Camping", "ForensicEvaluation",
        "TasteIdentification", "Peacemaking", "Provocation", "Enticement",
        "Musicianship", "ArmsLore", "Bowcraft", "Snooping", "Stealing"
    };

    public List<NpcTemplate> Parse(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return ParseContent(content);
    }

    public List<NpcTemplate> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        return ParseContent(content);
    }

    public List<NpcTemplate> ParseContent(string content)
    {
        var templates = new List<NpcTemplate>();
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

            // Look for NpcTemplate declaration
            if (line.StartsWith("NpcTemplate", StringComparison.OrdinalIgnoreCase))
            {
                var template = ParseTemplate(lines, ref lineIndex);
                if (template != null)
                {
                    templates.Add(template);
                }
            }
            else
            {
                lineIndex++;
            }
        }

        return templates;
    }

    private static bool IsComment(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("//") || trimmed.StartsWith('#');
    }

    private static NpcTemplate? ParseTemplate(string[] lines, ref int lineIndex)
    {
        var headerLine = lines[lineIndex].Trim();

        // Check if this is a commented-out template
        if (headerLine.StartsWith('#'))
        {
            SkipCommentedBlock(lines, ref lineIndex);
            return null;
        }

        // Parse template name: "NpcTemplate <name>" or "NpcTemplate <name> {"
        var parts = headerLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            lineIndex++;
            return null;
        }

        var templateName = parts[1].TrimEnd('{');
        var template = new NpcTemplate { TemplateName = templateName };

        lineIndex++;

        // Find opening brace if not on same line
        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line) || IsComment(line))
            {
                lineIndex++;
                continue;
            }
            if (line == "{" || headerLine.EndsWith('{'))
            {
                if (line == "{") lineIndex++;
                break;
            }
            lineIndex++;
            break;
        }

        // Parse properties until closing brace
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

            ParseProperty(template, line);
            lineIndex++;
        }

        return template;
    }

    private static void SkipCommentedBlock(string[] lines, ref int lineIndex)
    {
        var braceCount = 0;
        var foundOpenBrace = false;

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];

            if (line.Contains('{'))
            {
                foundOpenBrace = true;
                braceCount++;
            }
            if (line.Contains('}'))
            {
                braceCount--;
            }

            lineIndex++;

            if (foundOpenBrace && braceCount == 0)
            {
                break;
            }
        }
    }

    private static void ParseProperty(NpcTemplate template, string line)
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
            return;
        }

        // Split into tokens
        var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return;
        }

        var propertyName = tokens[0];
        var propertyNameLower = propertyName.ToLowerInvariant();

        // Handle CProp specially (3 tokens: CProp, name, value)
        if (propertyNameLower == "cprop" && tokens.Length >= 3)
        {
            var cpropName = tokens[1];
            var cpropValue = tokens[2];
            
            // Check for boss type CProps
            var cpropNameLower = cpropName.ToLowerInvariant();
            switch (cpropNameLower)
            {
                case "lesserboss":
                    template.BossType = BossType.LesserBoss;
                    break;
                case "boss":
                    template.BossType = BossType.Boss;
                    break;
                case "superboss":
                    template.BossType = BossType.SuperBoss;
                    break;
                case "champion":
                    template.BossType = BossType.Champion;
                    break;
                default:
                    template.CustomProperties[cpropName] = CPropertyValue.FromTypedString(cpropValue);
                    break;
            }
            return;
        }

        // Single value properties
        if (tokens.Length < 2)
        {
            return;
        }

        var value = tokens[1];

        // For Name property, join all remaining tokens (name can have spaces)
        if (propertyNameLower == "name")
        {
            template.Name = string.Join(" ", tokens.Skip(1));
            return;
        }

        // Multi-value properties
        switch (propertyNameLower)
        {
            case "spell":
                template.Spells.Add(value);
                return;
            case "equip":
                template.Equipment.Add(value);
                return;
        }

        // Check if it's a skill
        if (SkillNames.Contains(propertyName))
        {
            if (IntegerParser.TryParse(value, out var skillValue))
            {
                template.Skills[propertyName] = skillValue;
            }
            return;
        }

        // Parse standard properties
        switch (propertyNameLower)
        {
            case "script":
                template.Script = value;
                break;
            case "objtype":
                if (IntegerParser.TryParse(value, out var objType))
                    template.ObjType = objType;
                break;
            case "color":
                if (IntegerParser.TryParse(value, out var color))
                    template.Color = color;
                break;
            case "truecolor":
                if (IntegerParser.TryParse(value, out var trueColor))
                    template.TrueColor = trueColor;
                break;
            case "gender":
                if (IntegerParser.TryParse(value, out var gender))
                    template.Gender = gender;
                break;
            case "str":
                if (IntegerParser.TryParse(value, out var str))
                    template.Str = str;
                break;
            case "int":
                if (IntegerParser.TryParse(value, out var intVal))
                    template.Int = intVal;
                break;
            case "dex":
                if (IntegerParser.TryParse(value, out var dex))
                    template.Dex = dex;
                break;
            case "hits":
                if (IntegerParser.TryParse(value, out var hits))
                    template.Hits = hits;
                break;
            case "mana":
                if (IntegerParser.TryParse(value, out var mana))
                    template.Mana = mana;
                break;
            case "stam":
                if (IntegerParser.TryParse(value, out var stam))
                    template.Stam = stam;
                break;
            case "ar":
                if (IntegerParser.TryParse(value, out var ar))
                    template.Ar = ar;
                break;
            case "karma":
                if (IntegerParser.TryParse(value, out var karma))
                    template.Karma = karma;
                break;
            case "fame":
                if (IntegerParser.TryParse(value, out var fame))
                    template.Fame = fame;
                break;
            case "runspeed":
                if (IntegerParser.TryParse(value, out var runSpeed))
                    template.RunSpeed = runSpeed;
                break;
            case "alignment":
                template.Alignment = value;
                break;
            case "hostile":
                template.Hostile = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "virtue":
                if (IntegerParser.TryParse(value, out var virtue))
                    template.Virtue = virtue;
                break;
            case "guardignore":
                template.GuardIgnore = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "saywords":
                template.SayWords = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "speech":
                if (IntegerParser.TryParse(value, out var speech))
                    template.Speech = speech;
                break;
            case "provoke":
                if (IntegerParser.TryParse(value, out var provoke))
                    template.Provoke = provoke;
                break;
            case "dstart":
                if (IntegerParser.TryParse(value, out var dstart))
                    template.Dstart = dstart;
                break;
            case "tameskill":
                if (IntegerParser.TryParse(value, out var tameSkill))
                    template.TameSkill = tameSkill;
                break;
            case "food":
                template.Food = value;
                break;
            case "lootgroup":
                if (IntegerParser.TryParse(value, out var lootGroup))
                    template.LootGroupId = lootGroup;
                break;
            case "magicitemchance":
                if (IntegerParser.TryParse(value, out var magicChance))
                    template.MagicItemChance = magicChance;
                break;
            case "magicitemlevel":
                if (IntegerParser.TryParse(value, out var magicLevel))
                    template.MagicItemLevel = magicLevel;
                break;
            case "deathsnd":
                if (IntegerParser.TryParse(value, out var deathSnd))
                    template.DeathSound = deathSnd;
                break;
            case "privs":
                template.Privs = value;
                break;
            case "settings":
                template.Settings = value;
                break;
            case "noloot":
                template.NoLoot = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "dress":
                template.Dress = value;
                break;
            case "movemode":
                template.MoveMode = value;
                break;
            case "cast_pct":
                if (IntegerParser.TryParse(value, out var castPct))
                    template.CastPercent = castPct;
                break;
            case "num_casts":
                if (IntegerParser.TryParse(value, out var numCasts))
                    template.NumCasts = numCasts;
                break;
            case "ammotype":
                if (IntegerParser.TryParse(value, out var ammoType))
                    template.AmmoType = ammoType;
                break;
            case "missileweapon":
                if (IntegerParser.TryParse(value, out var missileWeapon))
                    template.MissileWeapon = missileWeapon;
                break;
            case "ammoamount":
                if (IntegerParser.TryParse(value, out var ammoAmount))
                    template.AmmoAmount = ammoAmount;
                break;
            case "attackattribute":
                template.AttackAttribute = value;
                break;
            case "attackspeed":
                if (IntegerParser.TryParse(value, out var attackSpeed))
                    template.AttackSpeed = attackSpeed;
                break;
            case "attackdamage":
                template.AttackDamage = value;
                break;
        }
    }
}
