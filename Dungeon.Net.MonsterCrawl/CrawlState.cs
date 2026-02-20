using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Dungeon.Net.MonsterCrawl;

public class CrawlState
{
    public int? PotionLastSelected = null;
    public int Health 
    { 
        get; 
        set
        {
            field = Math.Min(value, 20);
            HealthUpdated?.Invoke(); 
        } } = 20;
    public event Action? HealthUpdated;

    public Queue<Card> Deck { get; set; }

    public int? EquippedWeapon 
    { 
        get; 
        set
        {
            field = value;
            WeaponUpdated?.Invoke();
        } 
    }
    public int? MaxStrength
    { 
        get; 
        set
        {
            field = value;
            WeaponUpdated?.Invoke();
        } 
    }
    public event Action? WeaponUpdated;

    public CrawlState()
    {
        var cards = JsonSerializer.Deserialize<Card[]>(File.ReadAllText("./cards.json"));
        if(cards is null)
        {
            throw new InvalidDataException("Card data could not be loaded from json");
        }
        Deck = new(cards.OrderBy(_ => Random.Shared.Next()));
    }
}