using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var layout = new Layout("Root")
    .SplitRows(
        new Layout("HP").Size(1).Update(new Panel("")),
        new Layout("Room").Size(10).Update(new Panel("")),
        new Layout("Weapon").Size(10).Update(new Panel(""))
    );

await AnsiConsole.Live(layout)
    .StartAsync(async ctx =>
    {
        var provider = new ServiceCollection()
            .AddSingleton<GameState>()
            .AddSingleton<HealthBar>()
            .AddSingleton<Room>()
            .AddSingleton<Weapon>()
            .AddSingleton<InputManager>()
            .AddSingleton(ctx)
            .AddSingleton(layout)
            .BuildServiceProvider();
        layout["HP"].Update(Align.Center(provider.GetRequiredService<HealthBar>()));
        layout["Room"].Update(Align.Center(provider.GetRequiredService<Room>()));
        layout["Weapon"].Update(Align.Center(provider.GetRequiredService<Weapon>()));
        ctx.Refresh();
        var inputManager =  provider.GetRequiredService<InputManager>();
        await inputManager.StartListeningForInput();
    });
