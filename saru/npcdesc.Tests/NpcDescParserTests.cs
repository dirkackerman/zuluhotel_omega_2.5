using NpcDesc.Models;
using NpcDesc.Services;

namespace NpcDesc.Tests;

public class NpcDescParserTests
{
    private readonly NpcDescParser _parser = new();

    [Fact]
    public void ParseContent_SimpleTemplate_ParsesCorrectly()
    {
        var content = @"
NpcTemplate testmob
{
    Name        a Test Mob
    script      killpcs
    objtype     0x190
    Color       1234
    TrueColor   5678
    Gender      1
    STR         100
    INT         50
    DEX         75
    alignment   evil
    hostile     1
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal("testmob", template.TemplateName);
        Assert.Equal("a Test Mob", template.Name);
        Assert.Equal("killpcs", template.Script);
        Assert.Equal(0x190, template.ObjType);
        Assert.Equal(1234, template.Color);
        Assert.Equal(5678, template.TrueColor);
        Assert.Equal(1, template.Gender);
        Assert.Equal(100, template.Str);
        Assert.Equal(50, template.Int);
        Assert.Equal(75, template.Dex);
        Assert.Equal("evil", template.Alignment);
        Assert.True(template.Hostile);
    }

    [Fact]
    public void ParseContent_WithSkills_ParsesSkillsCorrectly()
    {
        var content = @"
NpcTemplate skillmob
{
    Name        Skilled Mob
    Parry       120
    Tactics     100
    Magery      150
    MaceFighting    80
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(120, template.Skills["Parry"]);
        Assert.Equal(100, template.Skills["Tactics"]);
        Assert.Equal(150, template.Skills["Magery"]);
        Assert.Equal(80, template.Skills["MaceFighting"]);
    }

    [Fact]
    public void ParseContent_WithSpells_ParsesSpellsCorrectly()
    {
        var content = @"
NpcTemplate caster
{
    Name        a Caster
    spell       flamestrike
    spell       ebolt
    spell       lightning
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(3, template.Spells.Count);
        Assert.Contains("flamestrike", template.Spells);
        Assert.Contains("ebolt", template.Spells);
        Assert.Contains("lightning", template.Spells);
    }

    [Fact]
    public void ParseContent_WithCProps_ParsesCPropsCorrectly()
    {
        var content = @"
NpcTemplate propmob
{
    Name        a Prop Mob
    CProp       Type            sHuman
    CProp       BaseStrmod      i100
    CProp       FireProtection  i-50
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal("Human", template.CustomProperties["Type"].StringValue);
        Assert.Equal(100, template.CustomProperties["BaseStrmod"].IntValue);
        Assert.Equal(-50, template.CustomProperties["FireProtection"].IntValue);
    }

    [Fact]
    public void ParseContent_WithEquipment_ParsesEquipmentCorrectly()
    {
        var content = @"
NpcTemplate equipped
{
    Name        an Equipped Mob
    Equip       balron1
    Equip       sword
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(2, template.Equipment.Count);
        Assert.Contains("balron1", template.Equipment);
        Assert.Contains("sword", template.Equipment);
    }

    [Fact]
    public void ParseContent_CommentedTemplate_IsSkipped()
    {
        var content = @"
# NpcTemplate commented
# {
#     Name    a Commented Mob
# }

NpcTemplate valid
{
    Name    a Valid Mob
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        Assert.Equal("valid", templates[0].TemplateName);
    }

    [Fact]
    public void ParseContent_WithInlineComments_RemovesComments()
    {
        var content = @"
NpcTemplate mob
{
    Name        a Mob // this is a comment
    STR         100 // strength value
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        Assert.Equal("a Mob", templates[0].Name);
        Assert.Equal(100, templates[0].Str);
    }

    [Fact]
    public void ParseContent_HexValues_ParsesCorrectly()
    {
        var content = @"
NpcTemplate hexmob
{
    Name        a Hex Mob
    objtype     0xc8
    Color       0xFF
    deathsnd    0x64
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        Assert.Equal(200, templates[0].ObjType); // 0xc8 = 200
        Assert.Equal(255, templates[0].Color); // 0xFF = 255
        Assert.Equal(100, templates[0].DeathSound); // 0x64 = 100
    }

    [Fact]
    public void ParseContent_MultipleTemplates_ParsesAll()
    {
        var content = @"
NpcTemplate mob1
{
    Name    First Mob
}

NpcTemplate mob2
{
    Name    Second Mob
}

NpcTemplate mob3
{
    Name    Third Mob
}";

        var templates = _parser.ParseContent(content);

        Assert.Equal(3, templates.Count);
        Assert.Equal("mob1", templates[0].TemplateName);
        Assert.Equal("mob2", templates[1].TemplateName);
        Assert.Equal("mob3", templates[2].TemplateName);
    }

    [Fact]
    public void ParseContent_WithLootGroup_ParsesLootGroupId()
    {
        var content = @"
NpcTemplate looter
{
    Name        a Looter
    lootgroup   17
    MagicItemChance     50
    MagicItemLevel      3
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        Assert.Equal(17, templates[0].LootGroupId);
        Assert.Equal(50, templates[0].MagicItemChance);
        Assert.Equal(3, templates[0].MagicItemLevel);
    }

    [Fact]
    public void ParseContent_WithBossCProp_ParsesBossType()
    {
        var content = @"
NpcTemplate bossmob
{
    Name        a Boss Mob
    CProp       Boss            i1
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(BossType.Boss, template.BossType);
        Assert.True(template.IsBoss);
        Assert.False(template.CustomProperties.ContainsKey("Boss"));
    }

    [Fact]
    public void ParseContent_WithSuperBossCProp_ParsesSuperBossType()
    {
        var content = @"
NpcTemplate superbossmob
{
    Name        a Super Boss Mob
    CProp       SuperBoss       i1
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(BossType.SuperBoss, template.BossType);
        Assert.True(template.IsBoss);
    }

    [Fact]
    public void ParseContent_WithLesserBossCProp_ParsesLesserBossType()
    {
        var content = @"
NpcTemplate lesserbossmob
{
    Name        a Lesser Boss Mob
    CProp       LesserBoss      i1
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(BossType.LesserBoss, template.BossType);
        Assert.True(template.IsBoss);
    }

    [Fact]
    public void ParseContent_WithChampionCProp_ParsesChampionType()
    {
        var content = @"
NpcTemplate championmob
{
    Name        a Champion Mob
    CProp       Champion        i1
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(BossType.Champion, template.BossType);
        Assert.True(template.IsBoss);
    }

    [Fact]
    public void ParseContent_WithNoBossCProp_BossTypeIsNone()
    {
        var content = @"
NpcTemplate normalmob
{
    Name        a Normal Mob
    CProp       Type            sAnimal
}";

        var templates = _parser.ParseContent(content);

        Assert.Single(templates);
        var template = templates[0];
        Assert.Equal(BossType.None, template.BossType);
        Assert.False(template.IsBoss);
    }
}
