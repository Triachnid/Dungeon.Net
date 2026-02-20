using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dungeon.Net.Engine;
using Spectre.Console;

public static class GameLoader
{
    private static PhaseRegistry? _registry;

    private class PhaseTemp
    {
        public ILayoutFactory? Factory { get; set; }
        public Dictionary<string, GameRenderableFactory> Components { get; set; } = [];
    }

    public static void LoadGame()
    {
        var phases = new Dictionary<int, PhaseTemp>();
        PhaseTemp GetOrAddDefinition(int phase)
        {
            if (!phases.TryGetValue(phase, out PhaseTemp? definition))
            {
                definition = new();
                phases.Add(phase, definition);
            }
            return definition;
        }
        foreach(var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if(type.GetInterface(nameof(ILayoutFactory)) is not null)
            { 
                var phaseAttribute = type.GetCustomAttribute<PhaseAttribute>();
                if(phaseAttribute is not null)
                {
                    var phase = GetOrAddDefinition(phaseAttribute.Sequence);
                    phase.Factory = Activator.CreateInstance(type) as ILayoutFactory;
                }
            }
            if(type.GetInterface(nameof(IGameRenderable)) is not null)
            {
                var componentAttribute = type.GetCustomAttribute<PhaseComponentAttribute>();
                if(componentAttribute is not null)
                {
                    var phase = GetOrAddDefinition(componentAttribute.Phase);
                    phase.Components.Add(componentAttribute.LayoutSection, (inputManager, ctx) =>
                    {
                        var component = (Activator.CreateInstance(type) as IGameRenderable)!;
                        component.ConfigureInput(inputManager, ctx);
                        return component;
                    });
                }
            }
        }
        _registry = new PhaseRegistry
        {
            Phases = phases.OrderBy(x => x.Key).Select(x => new PhaseDefinition
            {
                Components = x.Value.Components,
                LayoutFactory = x.Value.Factory!,
            })
        };
    }

    public static async Task StartLoadedGameAsync(CancellationToken cancellationToken)
    {
        if(_registry is null)
        {
            throw new InvalidOperationException();
        }
        
        await AnsiConsole.Live(new Text(""))
            .StartAsync(async ctx =>
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    foreach(var phase in _registry.Phases)
                    {
                        using var inputManager = new Dungeon.Net.Engine.InputManager();
                        var layout = phase.LayoutFactory.CreateLayout();
                        foreach(var component in phase.Components)
                        {
                            var instance = component.Value(inputManager, ctx);
                            layout[component.Key].Update(instance);
                        }
                        ctx.UpdateTarget(layout);
                        ctx.Refresh();
                        try
                        {
                            await inputManager.StartListenter();
                        } catch(TaskCanceledException){ }
                    }
                }
            });
    }
}