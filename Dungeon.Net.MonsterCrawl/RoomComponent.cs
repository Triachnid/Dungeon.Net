using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dungeon.Net.Engine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Dungeon.Net.MonsterCrawl;

[PhaseComponent(2, "Room")]
public class RoomComponent : AbstractGameRenderable
{
    private Card?[] _currentSet;
    private int _selectedIndex
    {
        get => field;
        set
        {
            var wrappedValue = value;
            if(wrappedValue < 0)
            {
                wrappedValue = _currentSet.Length-1;
            }
            else if(wrappedValue >= _currentSet.Length)
            {
                wrappedValue = 0;
            }
            _layout[$"C{field}"].Update(CreateCardPanel(_currentSet[field]));
            _layout[$"C{wrappedValue}"].Update(CreateCardPanel(_currentSet[wrappedValue], true));
            field = wrappedValue;
        }
    }

    private Card? _selectedCard 
        => _currentSet[_selectedIndex];
    
    private bool _lastRoomDiscarded = false;
    private bool _potionTaken = false;

    private Layout _layout;

    public RoomComponent()
    {
        _currentSet = new Card[4];
        _layout = new Layout("Cards")
            .SplitColumns(
                new Layout("C0"),
                new Layout("C1"),
                new Layout("C2"),
                new Layout("C3")
            );
        LoadRoom();
    }

    private void LoadRoom(bool discard = false)
    {
        _lastRoomDiscarded = discard;
        if(CrawlContext.State.Deck.Count < _currentSet.Length)
        {
            if (discard)
            {
                return;
            }
        }
        for (int i = 0; i < _currentSet.Length; i++)
        {
            var card = _currentSet[i];
            if (discard)
            {
                CrawlContext.State.Deck.Enqueue(_currentSet[i]!);
            }
            if(discard || card is null)
            {
                _currentSet[i] = CrawlContext.State.Deck.Dequeue();
                _layout[$"C{i}"].Update(CreateCardPanel(_currentSet[i]));
            }
        }
        _selectedIndex = 0;
        _potionTaken = false;
    }

    private static Panel CreateCardPanel(Card? card, bool selected = false)
    {
        var figlet = new FigletText(card?.Value.ToString()??"X").Centered();
        var panel = new Panel(figlet) { Width = 20 };
        if(card is not null)
        {
            panel.Header(card.CardType.ToString(), Justify.Left);
        }
            
        if (selected)
        {
            panel.DoubleBorder().BorderColor(Color.Green);
        }
        return panel;
    }

    protected override IRenderable Generate()
        => Align.Center(new Panel(Align.Center(_layout)){ Width = 90, Height = 10 }.Header("Room", Justify.Center));
    public override void ConfigureInput(Engine.InputManager inputManager, LiveDisplayContext ctx)
    {
        inputManager.RegisterAction(keyInfo =>
        {
            var movement = keyInfo.Key == ConsoleKey.LeftArrow ? -1 : 1;
            do
            {
                _selectedIndex += movement;
            } while(_selectedCard is null);
            ctx.Refresh();
        }, ConsoleKey.RightArrow, ConsoleKey.LeftArrow);
        inputManager.RegisterAction(_ =>
        {
            if(!_lastRoomDiscarded && !_currentSet.Any(x => x is null))
            {
                LoadRoom(true);
                _lastRoomDiscarded = true;
                ctx.Refresh();
            }
        }, ConsoleKey.Backspace);
        inputManager.RegisterAction(async _ =>
        {
            if(await ResolveSelectedCard(ctx, inputManager.InputCanceller))
            {
                _currentSet[_selectedIndex] = null;
                _layout[$"C{_selectedIndex}"].Update(CreateCardPanel(_selectedCard));
                if(_currentSet.Count(x => x is null) < 3)
                {
                    _selectedIndex++;
                }
                else if(CrawlContext.State.Deck.Count >= 0)
                {
                    LoadRoom();
                }
                else
                {
                    await inputManager.InputCanceller.CancelAsync();
                }
                ctx.Refresh();
            }
        }, ConsoleKey.Enter);

    }

    private async Task<bool> ResolveSelectedCard(LiveDisplayContext ctx, CancellationTokenSource inputCancellation)
    {
        if(_selectedCard is null)
        {
            return false;
        }
        CrawlContext.State.PotionLastSelected = null;
        switch (_selectedCard.CardType)
        {
            case CardType.Weapon:
                CrawlContext.State.EquippedWeapon = _selectedCard.Value;
                CrawlContext.State.MaxStrength = null;
                break;
            case CardType.Potion:
                if(!_potionTaken)
                {
                    CrawlContext.State.Health += _selectedCard.Value;
                    _potionTaken = true;    
                }
                CrawlContext.State.PotionLastSelected = _selectedCard.Value;
                break;
            case CardType.Monster:
                var damage = _selectedCard.Value;
                if(CrawlContext.State.EquippedWeapon is not null)
                {
                    var currentIndex = _selectedIndex;
                    bool? useWeapon = CrawlContext.State.MaxStrength < _selectedCard.Value
                        ? false
                        : ConfirmWeaponAttack();
                    if(useWeapon is null)
                    {
                        _layout[$"C{currentIndex}"].Update(CreateCardPanel(_currentSet[currentIndex]));
                        return false;
                    }
                    if (useWeapon.Value)
                    {
                        damage -= CrawlContext.State.EquippedWeapon.Value;
                        CrawlContext.State.MaxStrength = _selectedCard.Value;
                    }
                }
                if(damage > 0)
                {
                    CrawlContext.State.Health -= damage;
                }
                if(CrawlContext.State.Health <= 0)
                {
                    await inputCancellation.CancelAsync();
                }
                break;
        }

        return true;
        
        bool? ConfirmWeaponAttack()
        {
            var options = new Rows(new Text("X: Use Weapon"), new Text("C: Barehanded"));
            _layout[$"C{_selectedIndex}"].Update(new Panel(Align.Center(options)){ Width = 20 }.DoubleBorder().BorderColor(Color.Green));
            bool? result = null;
            ctx.Refresh();
            Engine.InputManager.ListenSingleInput(keyInfo =>
            {
                result = keyInfo.Key == ConsoleKey.X;
            }, ConsoleKey.X, ConsoleKey.C);
            return result;
        }
    }
}