namespace NpcDesc.Models;

public class NpcTemplate
{
    public string TemplateName { get; set; } = string.Empty;

    // Basic properties
    public string Name { get; set; } = string.Empty;
    public string? Script { get; set; }
    public int ObjType { get; set; }
    public int Color { get; set; }
    public int TrueColor { get; set; }
    public int Gender { get; set; }

    // Stats
    public int Str { get; set; }
    public int Int { get; set; }
    public int Dex { get; set; }
    public int? Hits { get; set; }
    public int? Mana { get; set; }
    public int? Stam { get; set; }
    public int? Ar { get; set; }

    // Combat/Behavior
    public int? Karma { get; set; }
    public int? Fame { get; set; }
    public int? RunSpeed { get; set; }
    public string? Alignment { get; set; }
    public bool Hostile { get; set; }
    public int? Virtue { get; set; }
    public bool GuardIgnore { get; set; }
    public bool SayWords { get; set; }
    public int? Speech { get; set; }
    public int? Provoke { get; set; }
    public int? Dstart { get; set; }
    public int? TameSkill { get; set; }
    public string? Food { get; set; }

    // Loot - Reference to loot group
    public int? LootGroupId { get; set; }
    public int? MagicItemChance { get; set; }
    public int? MagicItemLevel { get; set; }

    // Resolved loot group reference (populated after linking)
    public LootGroup? LootGroup { get; set; }

    // Misc
    public int? DeathSound { get; set; }
    public string? Privs { get; set; }
    public string? Settings { get; set; }
    public bool NoLoot { get; set; }
    public string? Dress { get; set; }
    public string? MoveMode { get; set; }

    // Casting
    public int? CastPercent { get; set; }
    public int? NumCasts { get; set; }

    // Ranged
    public int? AmmoType { get; set; }
    public int? MissileWeapon { get; set; }
    public int? AmmoAmount { get; set; }

    // Attack
    public string? AttackAttribute { get; set; }
    public int? AttackSpeed { get; set; }
    public string? AttackDamage { get; set; }

    // Skills (dictionary for flexibility)
    public Dictionary<string, int> Skills { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Equipment
    public List<string> Equipment { get; set; } = new();

    // Spells
    public List<string> Spells { get; set; } = new();

    // Custom Properties
    public Dictionary<string, CPropertyValue> CustomProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Helper methods
    public string? GetSlayerType() => 
        CustomProperties.TryGetValue("Type", out var val) ? val.StringValue : null;

    public bool IsBoss => 
        CustomProperties.ContainsKey("Boss") || 
        CustomProperties.ContainsKey("SuperBoss") || 
        CustomProperties.ContainsKey("LesserBoss") ||
        CustomProperties.ContainsKey("Champion");
}
