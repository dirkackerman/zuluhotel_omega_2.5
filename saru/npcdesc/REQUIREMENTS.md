# NpcDesc Parser Requirements Specification

## Overview

Parse the `npcdesc.cfg` and `nlootgroup.cfg` configuration files into an in-memory C# data model. The system should link NPC templates with their associated loot groups to provide a complete view of creature data including what loot they drop.

- `npcdesc.cfg`: ~31,000 lines with hundreds of NPC definitions
- `nlootgroup.cfg`: ~2,300 lines with loot group and item group definitions

---

# Part 1: NPC Template Parsing (npcdesc.cfg)

## File Format

### General Syntax

1. **Comments**: Lines starting with `//` or `#` are comments and should be ignored
2. **Commented-out blocks**: Entire NPC template blocks can be commented out with `#` prefix on each line
3. **Whitespace**: Properties are separated by tabs/spaces; the file uses inconsistent indentation
4. **Empty lines**: Should be ignored
5. **Case sensitivity**: Property names appear in mixed case (e.g., `STR`, `str`, `Str`) - parsing should be case-insensitive

### NPC Template Structure

```
NpcTemplate <template_name>
{
    <property> <value>
    <property> <value>
    ...
}
```

- Template names are single identifiers (no spaces), e.g., `beckon`, `earthelementalsummons`, `troll1`
- Opening brace `{` appears on the same line as `NpcTemplate` or on the next line
- Closing brace `}` appears on its own line
- Properties are indented with tabs/spaces

## Property Types

### 1. Simple Properties (Key-Value)

Single value after the property name:

| Property | Value Type | Example |
|----------|------------|---------|
| `Name` | string (may contain `<random>`) | `a Polar Bear`, `<random> the Banker` |
| `script` | string | `killpcs`, `merchant`, `barker` |
| `objtype` | integer (decimal or hex `0x`) | `0x190`, `176`, `0xc8` |
| `Color` | integer | `33784`, `0`, `1154` |
| `TrueColor` | integer | `33784`, `0` |
| `Gender` | integer (0 or 1) | `0` |
| `STR` | integer | `200` |
| `INT` | integer | `200` |
| `DEX` | integer | `175` |
| `HITS` | integer | `200` |
| `MANA` | integer | `200` |
| `STAM` | integer | `50` |
| `AR` | integer | Armor rating |
| `Karma` | integer (can be negative) | `-10000`, `2000` |
| `Fame` | integer | `10000` |
| `RunSpeed` | integer (1-250) | `250`, `200` |
| `alignment` | string | `good`, `evil` |
| `hostile` | integer (0 or 1) | `1` |
| `virtue` | integer (can be negative) | `-5`, `2`, `8` |
| `guardignore` | integer (0 or 1) | `1` |
| `saywords` | integer (0 or 1) | `1` |
| `speech` | integer | `7`, `28`, `35` |
| `provoke` | integer | `65`, `100`, `150` |
| `dstart` | integer | `10` |
| `tameskill` | integer | `40`, `200` |
| `food` | string | `veggie` |
| `lootgroup` | integer | `34`, `105` |
| `magicitemchance` / `MagicItemChance` | integer | `20`, `80`, `99` |
| `Magicitemlevel` / `MagicItemLevel` | integer | `2`, `9`, `10` |
| `deathsnd` | integer (hex) | `0x64`, `0xad` |
| `Privs` | string | `invul` |
| `Settings` | string | `invul` |
| `noloot` | integer | `1` |
| `dress` | string | `rich`, `poor` |
| `title` | prefixed value | `i1` |
| `MoveMode` | string | `AL` |
| `cast_pct` | integer | `80`, `100` |
| `num_casts` | integer | `8`, `40`, `50` |
| `ammotype` | integer (hex) | `0xf3f`, `0xEED` |
| `missileweapon` | integer (hex) | `0x13B2`, `0x9a0f` |
| `ammoamount` | integer | `35`, `300` |
| `mount` | string | mount identifier |
| `colorrange` | string | color range spec |
| `graphics` | string | graphics identifier |
| `buddyText` | string | text message |
| `targetText` | string | text message |

### 2. Skill Properties

Skills use the property name directly with an integer value (0-200+):

| Skill Property | Example Value |
|----------------|---------------|
| `Parry` | `120` |
| `Tactics` | `100` |
| `Swordsmanship` | `80` |
| `Magery` | `120` |
| `MaceFighting` | `150` |
| `Fencing` | `25` |
| `Archery` | `150` |
| `Wrestling` | `100` |
| `Healing` | `100` |
| `Anatomy` | `100` |
| `EvaluatingIntelligence` | `200` |
| `DetectingHidden` / `DetectHidden` | `200` |
| `Hiding` | `125` |
| `Stealth` | `100` |
| `Poisoning` | `100` |
| `SpiritSpeak` | `100` |
| `Meditation` | `100` |
| `MagicResistance` | `100` |
| `Alchemy` | `100` |
| `Blacksmithy` | `100` |
| `Tailoring` | `100` |
| `Carpentry` | `100` |
| `Tinkering` | `100` |
| `Mining` | `100` |
| `Lumberjacking` | `100` |
| `Fishing` | `100` |
| `Cooking` | `100` |
| `Inscription` | `100` |
| `Cartography` | `100` |
| `Lockpicking` | `100` |
| `RemoveTrap` | `100` |
| `Tracking` | `100` |
| `Veterinary` | `100` |
| `AnimalLore` | `100` |
| `AnimalTaming` | `100` |
| `Herding` | `100` |
| `Begging` | `100` |
| `Camping` | `100` |
| `Forensicevaluation` | `100` |
| `TasteIdentification` | `100` |
| `Peacemaking` | `100` |
| `Provocation` | `100` |
| `Enticement` | `100` |
| `Musicianship` | `100` |
| `ArmsLore` | `100` |
| `Bowcraft` | `100` |
| `Snooping` | `100` |
| `Stealing` | `100` |

### 3. Attack Properties

| Property | Value Type | Example |
|----------|------------|---------|
| `AttackAttribute` | string | `MaceFighting`, `Wrestling` |
| `AttackSpeed` | integer | `80` |
| `AttackDamage` | dice notation string | `5d100` |

### 4. Equip Property

```
Equip <equipment_template_name>
```

Example: `Equip balron1`, `Equip horse`, `Equip gardener`

Can appear multiple times in some templates (though typically once).

### 5. Spell Property (Multi-value)

```
spell <spell_name>
```

Can appear multiple times to define multiple spells:
```
spell ebolt
spell flamestrike
spell lightning
```

Examples of spell names: `ebolt`, `flamestrike`, `lightning`, `harm`, `curse`, `fireball`, `paralyze`, `chainlightning`, `meteorswarm`, `teleport`, `manavamp`, `mindblast`, `summonsshiftingearth`, `massgust`, `champfire`, etc.

### 6. CProp Property (Custom Properties)

```
CProp <property_name> <typed_value>
```

The value is prefixed with a type indicator:
- `i` = integer (e.g., `i100`, `i-50`)
- `s` = string (e.g., `sHuman`, `sFastest`, `sElemental`)

| CProp Name | Type | Example |
|------------|------|---------|
| `Type` | string | `sHuman`, `sAnimal`, `sDaemon`, `sUndead`, `sElemental`, `sPlant`, `sGargoyle`, `sTroll`, `sOrc`, etc. |
| `MoveSpeed` | string | `sFastest`, `sFast`, `sSlow` |
| `CustomHitsLevel` | int | `i100000`, `i6000000` |
| `BaseStrmod` | int | `i100`, `i2050` |
| `BaseIntmod` | int | `i10`, `i1900` |
| `BaseDexmod` | int | `i10`, `i250` |
| `BaseHpRegen` | int | `i20`, `i1000` |
| `BaseManaRegen` | int | `i100`, `i60000` |
| `PermPoisonImmunity` | int | `i3`, `i8` |
| `PermMagicImmunity` | int | `i1`, `i8` |
| `Permmr` | int | `i5`, `i6` |
| `FireProtection` | int | `i-50`, `i20`, `i200` |
| `WaterProtection` | int | `i-50`, `i20`, `i200` |
| `AirProtection` | int | `i-50`, `i20`, `i200` |
| `EarthProtection` | int | `i-50`, `i20`, `i200` |
| `NecroProtection` | int | `i20`, `i100` |
| `HolyProtection` | int | `i-100`, `i55`, `i95` |
| `PhysicalProtection` | int | `i1`, `i8`, `i16` |
| `PoisonProtection` | int | `i20` |
| `FreeAction` | int | `i1` |
| `Boss` | int | `i1` |
| `LesserBoss` | int | `i1` |
| `SuperBoss` | int | `i1` |
| `Champion` | int | `i1`, `i2`, `i3` |
| `IsMage` | int | `i2`, `i6` |
| `IsWarrior` | int | `i1` |
| `IsRanger` | int | `i1` |
| `IsThief` | int | `i1` |
| `snoopme` | int | `i40`, `i150` |
| `stealme` | int | `i40`, `i150` |
| `MerchantType` | string | `sthunter`, `sgardener`, `sflorist` |
| `Equipt` | string | `sMage`, `sclothes`, `sflorist` |
| `pack` | int | `i1` |
| `looter` | string | `s1` |
| `EBSummons` | int | `i1` |
| `event` | int | `i1` |
| `referee` | int | `i1` |
| `untamable` | int | `i1` |
| `nocorpse` | int | `i1` |
| `FinalDeath` | int | `i1` |
| `guardkill` | int | `i1` |
| `PeaceKeeper` | int | `i1` |
| `ignoremultis` | int | `i1` |
| `HealingIncrease` | int | `i50` |
| `AttackTypeImmunities` | string | `sArchery` |
| `wool` | int | `i1` |
| `rise` | int | `i1` |
| `risedelay` | int | `i60` |
| `target` | int | `i1` |
| `mountspawn` | string | mount template |
| `kappa` | int | `i1` |
| `vortexnaga` | int | `i1` |
| `WarriorForHire` | int | `i1` |
| `RefuseService` | string | `sred` |

---

# Part 2: Loot Group Parsing (nlootgroup.cfg)

## File Format

### General Syntax

1. **Comments**: Lines starting with `//` are comments and should be ignored during parsing
2. **Whitespace**: Properties are separated by tabs/spaces
3. **Empty lines**: Should be ignored
4. **Case sensitivity**: Entry type keywords should be case-insensitive

> **Note**: Comments in the loot group file (e.g., `//kobold1, orc1, ratman3`) are purely informational notes left by developers and should NOT be parsed or used for linking. The **only** authoritative link between an NPC and its loot group is the `lootgroup` property defined in the NPC template in `npcdesc.cfg`.

### Two Block Types

The file contains two types of definitions:

#### 1. Loot Groups (Numeric ID)

```
lootgroup <numeric_id>
{
    <entry_type> <amount> <item_or_group_name> [chance]
    ...
}
```

Example:
```
lootgroup 1
{
    Random      1           NormalArmor         30
    Random      1           NormalWeapons       40
    Random      1d2         Junk                50
    Stack       10d5        GoldCoin
    Random      1d2         Ammo                15
}
```

#### 2. Item Groups (Named)

```
Group <group_name>
{
    <entry_type> <amount> <item_name> [chance]
    ...
}
```

Example:
```
Group Reagents
{
    Stack   2d3     Garlic
    Stack   2d3     SpiderSilk
    Stack   2d3     MandrakeRoot
    ...
}
```

## Entry Types

Each line within a loot group or item group specifies one loot entry:

| Entry Type | Description | Format |
|------------|-------------|--------|
| `Stack` | Spawn stacked items (e.g., gold, reagents) | `Stack <amount> <item_name> [chance] [color]` |
| `Item` | Spawn a single item (or n copies) | `Item <amount> <item_name> [chance] [color]` |
| `Items` | Alias for `Item` (typo in file) | Same as `Item` |
| `Random` | Pick n items randomly from a group | `Random <amount> <group_name> [chance]` |
| `Group` | Include all entries from another group | `Group <group_name>` |
| `Unique` | Spawn at most one of this item | `Unique <item_name>` |

### Entry Parameters

| Parameter | Description | Format | Examples |
|-----------|-------------|--------|----------|
| `amount` | Quantity to spawn | Integer or dice notation | `1`, `2d3`, `1d5+3`, `10d5`, `1d300+250` |
| `item_name` | Item template name | String identifier | `GoldCoin`, `Longsword`, `level1map` |
| `group_name` | Reference to a Group | String identifier | `Reagents`, `NormalArmor`, `MagicWeapons` |
| `chance` | Spawn chance percentage (optional, default=100) | Integer 1-100 | `30`, `50`, `100` |
| `color` | Optional color override | Integer | (rarely used) |

### Dice Notation

Amount values support dice notation:
- `n` - Fixed number (e.g., `5`)
- `ndm` - Roll n dice with m sides (e.g., `2d6` = roll 2 six-sided dice)
- `ndm+b` - Roll with bonus (e.g., `1d100+50` = 1d100 plus 50)

Examples from file:
- `1` - Exactly 1
- `1d2` - 1 to 2
- `2d3` - 2 to 6
- `10d5` - 10 to 50
- `1d300+250` - 251 to 550
- `100d8` - 100 to 800

## Known Item Groups (by Name)

| Group Name | Description |
|------------|-------------|
| `Reagents` | Standard magic reagents (Garlic, SpiderSilk, etc.) |
| `PaganReagents` | Pagan magic reagents (BatWing, Bone, etc.) |
| `Gems` | Gemstones (amber, ruby, diamond, etc.) |
| `Ammo` | Arrows and bolts |
| `Ores` | Mining ores with varied chances |
| `NormalWeapons` | Standard weapons |
| `MagicWeapons` | Magic weapons (M-prefix, S-prefix, Stygian prefix) |
| `NormalArmor` | Standard armor pieces |
| `BoneArmor` | Bone armor set |
| `Clothes` | Clothing items |
| `Jewelry` | Rings and jewelry |
| `Junk` | Random junk items |
| `Circle1Scrolls` - `Circle8Scrolls` | Magic scrolls by circle |
| `PaganMagicScrolls` | Pagan magic scrolls |
| `Pentagrams` | Pentagram items (water, earth, air, fire, shadow, etc.) |
| `Books` | Spellbooks and tomes |
| `LowChance` | Rare book drops |
| `GMArmor` | Grandmaster armor |
| `GMWeapon` | Grandmaster weapons |
| `Eggs` | Monster eggs |
| `ChampionPresents` | Champion reward gifts |
| `Presents` | Gift items |
| `StygianShrine` | Stygian equipment set |

---

# Part 3: C# Data Model

## NPC Template Classes

```csharp
public class NpcTemplate
{
    public string TemplateName { get; set; }
    
    // Basic properties
    public string Name { get; set; }
    public string Script { get; set; }
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
    public string Alignment { get; set; }
    public bool Hostile { get; set; }
    public int? Virtue { get; set; }
    public bool GuardIgnore { get; set; }
    public bool SayWords { get; set; }
    public int? Speech { get; set; }
    public int? Provoke { get; set; }
    public int? Dstart { get; set; }
    public int? TameSkill { get; set; }
    public string Food { get; set; }
    
    // Loot - Reference to loot group
    public int? LootGroupId { get; set; }
    public int? MagicItemChance { get; set; }
    public int? MagicItemLevel { get; set; }
    
    // Resolved loot group reference (populated after linking)
    public LootGroup LootGroup { get; set; }
    
    // Misc
    public int? DeathSound { get; set; }
    public string Privs { get; set; }
    public string Settings { get; set; }
    public bool NoLoot { get; set; }
    public string Dress { get; set; }
    public string MoveMode { get; set; }
    
    // Casting
    public int? CastPercent { get; set; }
    public int? NumCasts { get; set; }
    
    // Ranged
    public int? AmmoType { get; set; }
    public int? MissileWeapon { get; set; }
    public int? AmmoAmount { get; set; }
    
    // Attack
    public string AttackAttribute { get; set; }
    public int? AttackSpeed { get; set; }
    public string AttackDamage { get; set; }
    
    // Skills (dictionary for flexibility)
    public Dictionary<string, int> Skills { get; set; } = new();
    
    // Equipment
    public List<string> Equipment { get; set; } = new();
    
    // Spells
    public List<string> Spells { get; set; } = new();
    
    // Custom Properties
    public Dictionary<string, CPropertyValue> CustomProperties { get; set; } = new();
}

public class CPropertyValue
{
    public CPropertyType Type { get; set; }
    public int? IntValue { get; set; }
    public string StringValue { get; set; }
}

public enum CPropertyType
{
    Integer,
    String
}
```

## Loot Group Classes

```csharp
/// <summary>
/// Represents a loot group definition (numeric ID based)
/// </summary>
public class LootGroup
{
    public int Id { get; set; }
    public List<LootEntry> Entries { get; set; } = new();
}

/// <summary>
/// Represents a named item group that can be referenced by loot groups
/// </summary>
public class ItemGroup
{
    public string Name { get; set; }
    public List<LootEntry> Entries { get; set; } = new();
}

/// <summary>
/// Base class for loot entries
/// </summary>
public abstract class LootEntry
{
    public LootEntryType EntryType { get; set; }
    public DiceRoll Amount { get; set; }
    public int ChancePercent { get; set; } = 100;
}

/// <summary>
/// Entry that spawns a specific item
/// </summary>
public class ItemLootEntry : LootEntry
{
    public string ItemName { get; set; }
    public int? Color { get; set; }
    public bool IsStack { get; set; }  // true for Stack, false for Item
    public bool IsUnique { get; set; } // true for Unique entries
}

/// <summary>
/// Entry that randomly picks from another group
/// </summary>
public class RandomGroupEntry : LootEntry
{
    public string GroupName { get; set; }
    
    // Resolved reference (populated after linking)
    public ItemGroup ResolvedGroup { get; set; }
}

/// <summary>
/// Entry that includes all items from another group
/// </summary>
public class IncludeGroupEntry : LootEntry
{
    public string GroupName { get; set; }
    
    // Resolved reference (populated after linking)
    public ItemGroup ResolvedGroup { get; set; }
}

public enum LootEntryType
{
    Stack,      // Stacked items (gold, reagents)
    Item,       // Individual item(s)
    Random,     // Random pick from group
    Group,      // Include entire group
    Unique      // At most one
}

/// <summary>
/// Represents a dice roll expression (e.g., "2d6+3")
/// </summary>
public class DiceRoll
{
    public int NumDice { get; set; } = 1;
    public int DiceSides { get; set; } = 1;
    public int Bonus { get; set; } = 0;
    
    /// <summary>
    /// Parse dice notation string (e.g., "2d6", "1d100+50", "5")
    /// </summary>
    public static DiceRoll Parse(string notation);
    
    /// <summary>
    /// Calculate minimum possible roll
    /// </summary>
    public int MinValue => NumDice + Bonus;
    
    /// <summary>
    /// Calculate maximum possible roll
    /// </summary>
    public int MaxValue => (NumDice * DiceSides) + Bonus;
    
    /// <summary>
    /// Roll the dice and return result
    /// </summary>
    public int Roll(Random rng = null);
    
    public override string ToString() => 
        DiceSides == 1 ? $"{NumDice + Bonus}" :
        Bonus == 0 ? $"{NumDice}d{DiceSides}" : 
        $"{NumDice}d{DiceSides}+{Bonus}";
}
```

## Parser Classes

```csharp
public class NpcDescParser
{
    public List<NpcTemplate> Parse(string filePath);
    public List<NpcTemplate> Parse(Stream stream);
    public List<NpcTemplate> ParseContent(string content);
}

public class LootGroupParser
{
    public LootGroupCollection Parse(string filePath);
    public LootGroupCollection Parse(Stream stream);
    public LootGroupCollection ParseContent(string content);
}

public class LootGroupCollection
{
    public Dictionary<int, LootGroup> LootGroups { get; } = new();
    public Dictionary<string, ItemGroup> ItemGroups { get; } = new();
    
    public LootGroup GetLootGroup(int id);
    public ItemGroup GetItemGroup(string name);
    
    /// <summary>
    /// Resolve all group references after parsing
    /// </summary>
    public void ResolveReferences();
    
    /// <summary>
    /// Get all possible items from a loot group (recursively resolving groups)
    /// </summary>
    public IEnumerable<PossibleLootItem> GetPossibleItems(int lootGroupId);
}

/// <summary>
/// Represents a possible item drop with calculated chance
/// </summary>
public class PossibleLootItem
{
    public string ItemName { get; set; }
    public DiceRoll Amount { get; set; }
    public double EffectiveChance { get; set; }  // Combined chance through group hierarchy
    public bool IsStack { get; set; }
    public List<string> SourcePath { get; set; }  // Path through groups to reach this item
}
```

## Collection and Linking Classes

```csharp
public class NpcTemplateCollection
{
    public List<NpcTemplate> Templates { get; }
    public LootGroupCollection LootGroups { get; }
    
    public NpcTemplate GetByName(string templateName);
    public IEnumerable<NpcTemplate> GetByType(string type);
    public IEnumerable<NpcTemplate> GetByAlignment(string alignment);
    public IEnumerable<NpcTemplate> GetBosses();
    public IEnumerable<NpcTemplate> GetByLootGroup(int lootGroupId);
    
    /// <summary>
    /// Link NPC templates to their loot groups
    /// </summary>
    public void LinkLootGroups();
    
    /// <summary>
    /// Get all possible loot for an NPC
    /// </summary>
    public IEnumerable<PossibleLootItem> GetPossibleLoot(string templateName);
}

/// <summary>
/// Main entry point for loading all configuration
/// </summary>
public class GameConfigLoader
{
    public NpcTemplateCollection LoadAll(string npcDescPath, string lootGroupPath);
}
```

---

# Part 4: Parsing Requirements

## NPC Parsing Must Handle

1. **Hex values**: Parse `0x190` as integer 400
2. **Negative values**: Support negative integers like `i-50` or `-10000`
3. **Case insensitivity**: Property names should match regardless of case
4. **Duplicate properties**: Some properties like `Equip`, `spell` can appear multiple times - collect into lists
5. **Missing properties**: Most properties are optional; use nullable types or defaults
6. **Inline comments**: Handle `//` comments that may appear after values
7. **Commented templates**: Skip templates where lines are prefixed with `#`
8. **Whitespace variations**: Handle tabs, multiple spaces, inconsistent indentation
9. **Special name tokens**: Handle `<random>` in Name values
10. **Value prefixes**: Parse `i` and `s` prefixes in CProp values

## Loot Group Parsing Must Handle

1. **Dice notation**: Parse all forms: `n`, `ndm`, `ndm+b`
2. **Optional chance**: Default to 100% if not specified
3. **Entry type aliases**: Handle `Items` as alias for `Item`
4. **Group references**: Track references to other groups for later resolution
5. **Missing parameters**: Handle entries with minimal parameters (e.g., `Group Reagents` with no amount)
6. **Case insensitivity**: Entry type keywords should be case-insensitive

## Error Handling

1. Report line number for parsing errors
2. Continue parsing after non-fatal errors (log warning, skip malformed entry)
3. Provide summary of skipped/failed entries after parsing
4. Warn on unresolved group references
5. Detect circular group references

## Performance

1. Both files should parse in under 1 second total
2. Consider lazy resolution of group references if needed

---

# Part 5: Console Application Requirements

The console app should:

1. Accept config file paths as command-line arguments (or use default paths)
2. Parse both files and link them together
3. Display summary statistics:
   - Total NPC templates parsed
   - Templates by type (slayer type)
   - Templates by alignment
   - Boss counts
   - Total loot groups parsed
   - Total item groups parsed
   - Any parsing errors/warnings
4. Support queries:
   - Look up NPC by template name
   - Show NPC details including resolved loot
   - List all possible drops for an NPC
   - Find NPCs by loot group ID

Example output:
```
$ npcdesc --npc troll1

NPC Template: troll1
  Name: a Troll
  Type: sTroll
  STR: 175, INT: 25, DEX: 60
  Alignment: evil
  Hostile: true
  
  Loot Group: 17
    - Battleaxe (100%)
    - 1 NormalArmor (30%) -> [from group: LeatherTunic, ChainmailCoif, ...]
    - 1 NormalWeapons (40%) -> [from group: Longsword, WarAxe, ...]
    - 1d2 Junk (50%)
    - 2 Gems (100%)
    - level1map (1%)
    - 10d10+75 GoldCoin (100%) -> 85-175 gold
```

---

# Part 6: Testing Requirements

1. Unit tests for parser edge cases:
   - Dice notation parsing
   - Hex value parsing
   - CProp value parsing
   - Comment handling
2. Integration test with actual config files
3. Verify all templates parse without errors
4. Verify all loot groups parse without errors
5. Verify group references resolve correctly
6. Validate NPC-to-loot-group linking
