using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Dungeon.Net.Engine;

public interface IGameRenderable : IRenderable
{
    public void Regenerate();
    public void ConfigureInput(InputManager inputManager, LiveDisplayContext ctx);
}

public abstract class AbstractGameRenderable : IGameRenderable
{
    private Lazy<IRenderable> _renderable;

    protected AbstractGameRenderable()
        => _renderable = new(Generate);

    protected abstract IRenderable Generate();

    public void Regenerate()
    {
        _renderable = new(Generate);
    }

    public virtual void ConfigureInput(InputManager inputManager, LiveDisplayContext ctx) { }

    public Measurement Measure(RenderOptions options, int maxWidth)
        => _renderable.Value.Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => _renderable.Value.Render(options, maxWidth);
}