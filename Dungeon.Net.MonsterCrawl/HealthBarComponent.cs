using Dungeon.Net.Engine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Dungeon.Net.MonsterCrawl;

[PhaseComponent(2, "HP")]
public class HealthBarComponent : AbstractGameRenderable
{
    public HealthBarComponent()
    {
        CrawlContext.State.HealthUpdated += Regenerate;
    }

    protected override IRenderable Generate()
        => Align.Center(new Panel(Align.Center(new BarChart().AddItem("HP", CrawlContext.State.Health, Color.Red).WithMaxValue(20).Width(90)))
        {
            Height = 3
        }.NoBorder());
}