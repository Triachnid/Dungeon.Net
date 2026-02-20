using Dungeon.Net.Engine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Dungeon.Net.MonsterCrawl;

[PhaseComponent(2, "Weapon")]
public class WeaponComponent : AbstractGameRenderable
{
    private readonly Layout _layout;

    public WeaponComponent()
    {
        _layout = new Layout("Weapon").SplitColumns(
            new Layout("Equipped"),
            new Layout("Max")
        );
        CrawlContext.State.WeaponUpdated += Regenerate;
    }

    protected override IRenderable Generate()
    {
        _layout["Equipped"].Update(GetSubPanel(CrawlContext.State.EquippedWeapon?.ToString(), "Weapon"));
        _layout["Max"].Update(GetSubPanel(CrawlContext.State.MaxStrength?.ToString(), "Max"));
        return Align.Center(new Panel(Align.Center(_layout)){ Width = 50, Height = 10 }.HeavyBorder().Header("Equipped", Justify.Center));

        static Align GetSubPanel(string? content, string header)
            => Align.Center(new Panel(new FigletText(content??"X").Centered()){ Width = 20}.Header(header, Justify.Left));
    }
}