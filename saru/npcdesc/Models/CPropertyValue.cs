namespace NpcDesc.Models;

public record CPropertyValue
{
    public CPropertyType Type { get; init; }
    public int? IntValue { get; init; }
    public string? StringValue { get; init; }

    public static CPropertyValue FromTypedString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new CPropertyValue { Type = CPropertyType.String, StringValue = value };
        }

        var prefix = value[0];
        var remainder = value.Length > 1 ? value[1..] : string.Empty;

        return prefix switch
        {
            'i' when int.TryParse(remainder, out var intVal) => 
                new CPropertyValue { Type = CPropertyType.Integer, IntValue = intVal },
            's' => 
                new CPropertyValue { Type = CPropertyType.String, StringValue = remainder },
            _ => 
                new CPropertyValue { Type = CPropertyType.String, StringValue = value }
        };
    }

    public override string ToString()
    {
        return Type switch
        {
            CPropertyType.Integer => $"i{IntValue}",
            CPropertyType.String => $"s{StringValue}",
            _ => StringValue ?? string.Empty
        };
    }
}
