using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Rendering;

public class Room : IRenderable
{
    //private readonly Queue<RenderableCard> _deck;
    //private RenderableCard[] _currentCards = new RenderableCard[4];
    //private int _currentIndex = 0;

    // public bool AnySelected => _selectedCount > 0;
    // private int _selectedCount = 0;
    private Panel _panel;
    private readonly GameState _state;
    private readonly InputManager _inputManager;

    public Room(GameState state, InputManager inputManager)
    {
        _state = state;
        _inputManager = inputManager;
        _panel = LoadRoom();
        ConfigureInput();
    }

    private Panel LoadRoom()
        => new Panel(new Columns(_state.CurrentRoom)){ Width = 90 }.Header("Room", Justify.Center);

    private void UpdateRoom()
    {
        _panel = LoadRoom();
        _state.LiveDisplayContext.Refresh();
    }

    private void ConfigureInput()
    {
        _inputManager
            .RegisterKey(ConsoleKey.RightArrow, () =>
            {
                _state.SelectNextCard();
                UpdateRoom();
            })
            .RegisterKey(ConsoleKey.LeftArrow, () =>
            {
                _state.SelectPreviousCard();
                UpdateRoom();
            })
            .RegisterKey(ConsoleKey.Enter, () =>
            {
                _state.SelectCurrentCard();
                UpdateRoom();
            });
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => (_panel as IRenderable).Render(options, maxWidth);
}