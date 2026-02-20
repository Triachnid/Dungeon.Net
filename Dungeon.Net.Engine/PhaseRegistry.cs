using System.Collections.Generic;
using Spectre.Console;

namespace Dungeon.Net.Engine;

internal delegate IGameRenderable GameRenderableFactory(InputManager inputManager, LiveDisplayContext ctx);

internal class PhaseDefinition
{
    public required ILayoutFactory LayoutFactory { get; init; }
    public required IReadOnlyDictionary<string, GameRenderableFactory> Components { get; init; }
}

internal class PhaseRegistry
{
    public required IEnumerable<PhaseDefinition> Phases { get; init; }
}