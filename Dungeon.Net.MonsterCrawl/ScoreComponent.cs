using System;
using System.Linq;
using Dungeon.Net.Engine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Dungeon.Net.MonsterCrawl;

[PhaseComponent(3, "Root")]
public class ScoreComponent : AbstractGameRenderable
{
    protected override IRenderable Generate()
    {
        var fail = CrawlContext.State.Deck.Count > 0;
        var points = CrawlContext.State.Health;
        if (fail)
        {
            var remainingMonsterTotal = CrawlContext.State.Deck.Where(x => x.CardType == CardType.Monster).Sum(x => x.Value);
            points -= remainingMonsterTotal;
        }
        else if (CrawlContext.State.PotionLastSelected is not null)
        {
            points += CrawlContext.State.PotionLastSelected.Value;
        }
        return Align.Center(new Panel(
            $"""
            Game Over.
            Score: {points}
            Press Enter to play again.
            Press Esc to exit.
            """
        ));
    }

    public override void ConfigureInput(Engine.InputManager inputManager, LiveDisplayContext ctx)
    {
        inputManager.RegisterAction(async (keyInfo) =>
        {
            if(keyInfo.Key == ConsoleKey.Escape)
            {
                CrawlContext.GameEndTokenSource.Cancel();
            }
            else
            {
                CrawlContext.State = new();
            }
            await inputManager.InputCanceller.CancelAsync();
        }, ConsoleKey.Enter, ConsoleKey.Escape);
    }
}