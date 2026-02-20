using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

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

public class RenderableCard : IRenderable
{
    private Panel _panel;
    public Card Card { get; init; }
    public bool Removed { get; private set; } = false;

    public RenderableCard(Card card)
    {
        Card = card;
        _panel = CreatePanel(card.Value.ToString(), card.CardType.ToString());
    }

    public bool ConfirmWeaponAttack(LiveDisplayContext ctx)
    {
        var options = new Rows(new Text("X: Use Weapon"), new Text("C: Barehanded"));
        _panel = new Panel(Align.Center(options)){ Width = 20 }.DoubleBorder().BorderColor(Color.Green);
        var result = false;
        using var inputManager = new InputManager();
        ctx.Refresh();
        inputManager
            .RegisterKey(ConsoleKey.X, () =>
            {
                result = true;
            })
            .RegisterKey(ConsoleKey.C, () =>
            {
                result = false;
            });
        inputManager.ListenForNextInput();
        Remove();
        ctx.Refresh();
        return result;
    }

    private Panel CreatePanel(string value, string? header = null)
    {
        var panel = new Panel(new FigletText(value).Centered()) { Width = 20 };
        if(header is not null)
        {
            panel.Header(header, Justify.Left);
        }
        return panel;
    }

    public RenderableCard Deselect()
    {
        _panel.SquareBorder().BorderColor(Color.White);
        return this;
    }

    public RenderableCard Select()
    {
        _panel.DoubleBorder().BorderColor(Color.Green);
        return this;
    }

    public RenderableCard Remove()
    {
        Removed = true;
        _panel = CreatePanel("X");
        return this;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Measure(options, maxWidth);

    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Render(options, maxWidth);
}

public static class CardLoader
{
    public static Queue<Card> LoadShuffledCards()
    {
        var cards = JsonSerializer.Deserialize<Card[]>(File.ReadAllText("./cards.json"));
        if(cards is null)
        {
            throw new InvalidDataException("Card data could not be loaded from json");
        }
        var cardCanvases = cards.OrderBy(_ => Random.Shared.Next());
        return new Queue<Card>(cardCanvases);
    }

    public static Queue<RenderableCard> ShuffleCards()
    {
        var cards = JsonSerializer.Deserialize<Card[]>(File.ReadAllText("./cards.json"));
        if(cards is null)
        {
            throw new InvalidDataException("Card data could not be loaded from json");
        }
        var renderableCards = cards
            .OrderBy(_ => Random.Shared.Next())
            .Select(x => new RenderableCard(x));
        return new Queue<RenderableCard>(renderableCards);
    }
}