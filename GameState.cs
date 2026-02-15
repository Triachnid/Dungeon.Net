using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

public class GameState
{
    public int Health { get; set; } = 20;
    public bool PotionTaken { get; set; } = false;

    public Queue<RenderableCard> Deck { get; init; }

    public bool PreviouslyDiscardedRoom { get; set; }
    public RenderableCard[] CurrentRoom { get; }

    public int SelectedIndex { get; set; } = 0;

    public RenderableCard SelectedRenderableCard
        => CurrentRoom[SelectedIndex];

    public Card SelectedCard
        => SelectedRenderableCard.Card;

    public Card? EquippedWeapon { get; set; }
    
    public int? MaxAttack { get; set; }

    public int SelectedCardCount
        => CurrentRoom.Count(x => x.Removed);

    public bool AnySelected
        => CurrentRoom.Any(x => x.Removed);

    public LiveDisplayContext LiveDisplayContext { get; }
    public Layout Layout { get; }

    public event Action WeaponUpdated = ()=>{};
    public event Action HealthUpdated = ()=>{};

    private readonly InputManager _inputManager;

    public GameState(LiveDisplayContext liveDisplayContext, Layout layout, InputManager inputManager)
    {
        Deck = CardLoader.ShuffleCards();
        CurrentRoom = new RenderableCard[4];
        LiveDisplayContext = liveDisplayContext;
        NewRoom();
        Layout = layout;
        _inputManager = inputManager;
    }

    public void NewRoom(bool discard = false)
    {
        PreviouslyDiscardedRoom = discard;
        if(Deck.Count < CurrentRoom.Length)
        {
            EndGame(false);
        }
        for (int i = 0; i < CurrentRoom.Length; i++)
        {
            var card = CurrentRoom[i];
            if (discard)
            {
                Deck.Enqueue(CurrentRoom[i]);
                CurrentRoom[i] = Deck.Dequeue();
            }
            else if(card is null || card.Removed)
            {
                CurrentRoom[i] = Deck.Dequeue();
            }
        }
        SelectedIndex = 0;
        PotionTaken = false;
        SelectedRenderableCard.Select();
    }

    public void SelectNextCard()
    {
        SelectedRenderableCard.Deselect();
        var first = true;
        while(first || SelectedRenderableCard.Removed){
            first = false;
            SelectedIndex++;
            if(SelectedIndex > CurrentRoom.Length-1)
            {
                SelectedIndex = 0;
            }
        }
        SelectedRenderableCard.Select();
    }

    public void SelectPreviousCard()
    {
        SelectedRenderableCard.Deselect();
        var first = true;
        while(first || SelectedRenderableCard.Removed){
            first = false;
            SelectedIndex--;
            if(SelectedIndex < 0)
            {
                SelectedIndex = CurrentRoom.Length-1;
            }
        }
        SelectedRenderableCard.Select();
    }

    public void SelectCurrentCard()
    {
        ResolveSelectedCard();
        SelectedRenderableCard.Remove();
        if(SelectedCardCount < 3)
        {
            SelectNextCard();
        }
        else
        {
            NewRoom();
        }
    }

    private void ResolveSelectedCard()
    {
        switch (SelectedCard.CardType)
        {
            case CardType.Weapon:
                EquippedWeapon = SelectedCard;
                MaxAttack = null;
                WeaponUpdated();
                break;
            case CardType.Monster:
                var damage = SelectedCard.Value;
                if(EquippedWeapon is not null)
                {
                    bool useWeapon;
                    if(MaxAttack < SelectedCard.Value)
                    {
                        useWeapon = false;
                    }
                    else
                    {
                        useWeapon = SelectedRenderableCard.ConfirmWeaponAttack(LiveDisplayContext);
                    }
                    if (useWeapon)
                    {
                        damage -= EquippedWeapon.Value;
                        MaxAttack = SelectedCard.Value;
                        WeaponUpdated();
                    }
                }
                if(damage > 0){
                    Health -= damage;
                    if(Health <= 0)
                    {
                        EndGame(true);
                    }
                    HealthUpdated();
                }
                break;
            case CardType.Potion:
                if (!PotionTaken)
                {
                    Health += SelectedCard.Value;
                    if(Health > 20)
                    {
                        Health = 20;
                    }
                    HealthUpdated();
                    PotionTaken = true;
                }
                break;
        }
        LiveDisplayContext.Refresh();
    }

    public bool ConfirmUseWeapon()
        => SelectedRenderableCard.ConfirmWeaponAttack(LiveDisplayContext);

    private void EndGame(bool fail)
    {
        var points = Health;
        if (fail)
        {
            var remainingMonsterTotal = Deck.Where(x => x.Card.CardType == CardType.Monster).Sum(x => x.Card.Value);
            points = Health - remainingMonsterTotal;
        }
        else if (SelectedCard.CardType == CardType.Potion)
        {
            points += SelectedCard.Value;
        }
        
        LiveDisplayContext.UpdateTarget(Align.Center(new Panel(
            $"""
            Game Over. 
            Score: {points}
            Press any key to Exit.
            """)));
        LiveDisplayContext.Refresh();

        var finalInputManager = new InputManager();
        finalInputManager.ListenForNextInput();
        _inputManager.Dispose();
    }
}