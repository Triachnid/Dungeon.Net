using System.Text.Json.Serialization;

namespace Dungeon.Net.MonsterCrawl;

public enum CardType
{
    Monster,
    Weapon,
    Potion,
}

public class Card
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardType CardType { get; init; }
    public required int Value { get; init; }
}