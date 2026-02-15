using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public class HealthBar : IRenderable
{
    private Panel _panel;
    private readonly GameState _state;

    public HealthBar(GameState state)
    {
        _state = state;
        _panel = CreatePanel();
        _state.HealthUpdated += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        _panel = CreatePanel();
    }

    private Panel CreatePanel()
        => new Panel(Align.Center(new BarChart().AddItem("HP", _state.Health, Color.Red).WithMaxValue(20).Width(90)))
        {
            Height = 3
        }.NoBorder();

    public Measurement Measure(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Render(options, maxWidth);
}