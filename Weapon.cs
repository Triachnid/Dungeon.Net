using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public class Weapon : IRenderable
{
    private Panel _panel;
    private readonly Layout _layout;
    private readonly GameState _state;

    public Weapon(GameState state)
    {
        _layout = new Layout("Root").SplitColumns(
            new Layout("Weapon").Update(GetSubPanel("X", "Weapon")),
            new Layout("Max").Update(GetSubPanel("X", "Max"))
        );
        _panel = new Panel(Align.Center(_layout)) { Width = 50, Height = 10 }.HeavyBorder().Header("Equipped", Justify.Center);
        _state = state;
        _state.WeaponUpdated += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        _layout["Weapon"].Update(GetSubPanel(_state.EquippedWeapon?.Value.ToString()??"X", "Weapon"));
        _layout["Max"].Update(GetSubPanel(_state.MaxAttack?.ToString()??"X", "Max"));
    }

    private static Align GetSubPanel(string content, string header)
        => Align.Center(new Panel(new FigletText(content).Centered()){ Width = 20 }.Header(header, Justify.Left));

    public Measurement Measure(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Render(options, maxWidth);
}