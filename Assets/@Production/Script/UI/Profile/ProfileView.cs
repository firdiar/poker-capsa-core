using DG.Tweening;
using Gtion.Plugin.DI;
using Pker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public enum Reaction
{ 
    Happy = 0,
    Angry = 1,
    PlayCard = 2,
    SkipTurn = 3,
    NewPlayer = 4,
}

public class ProfileView : MonoBehaviour, IInjectCallback
{
    [SerializeField]
    int playerId;

    [SerializeField]
    TextMeshProUGUI moneyText;
    [SerializeField]
    TextMeshProUGUI cardAmount;
    [SerializeField]
    Image profile;
    [SerializeField]
    Image emoticon;
    [SerializeField]
    Image playIndicator;

    [SerializeField]
    Sprite[] photoProfiles;

    [SerializeField]
    Sprite[] reaction;

    [GInject]
    PokerGameManager pokerManager;

    PokerPlayer pokerPlayer => pokerManager.PokerPlayers[playerId];

    public bool IsDependencyReady { get; set; }

    public async void OnDependencyReady()
    {
        OnPlayerChanged(playerId);
        pokerManager.OnPlayerAction.AddListener(OnPlayCard);
        pokerManager.OnPlayerChanged.AddListener(OnPlayerChanged);
        pokerManager.OnPlayerWin.AddListener(OnPlayerWin);
        pokerManager.OnPlayerWinSession.AddListener(OnPlayerWinSession);
        pokerManager.OnTurnChange.AddListener(OnTurnChange);
        pokerManager.OnNewCardShared.AddListener(Initialize);

        await pokerManager.WaitUntilGameStart();
        
        Initialize();
    }

    private void OnTurnChange(int arg0)
    {
        cardAmount.SetText(pokerPlayer.Cards.Count.ToString());
        if (arg0 == playerId)
        {
            playIndicator.DOFade(1, 0.3f);
        }
        else
        {
            playIndicator.DOFade(0, 0.3f);
        }
    }

    private void OnPlayCard(int arg0, PlayerAction arg1)
    {
        if (arg0 == playerId)
        {
            ShowReaction(arg1 == PlayerAction.Play ? Reaction.PlayCard : Reaction.SkipTurn);
        }
    }
    private void OnPlayerWinSession(int arg0)
    {
        if (arg0 == playerId)
        {
            ShowReaction(Reaction.Happy);
        }
    }
    private void OnPlayerWin(int arg0)
    {
        if (arg0 != playerId)
        {
            ShowReaction(Reaction.Angry);
        }
        else
        {
            ShowReaction(Reaction.Happy);
        }

        Initialize();
    }

    private void OnPlayerChanged(int arg0)
    {
        if (arg0 == playerId)
        {
            profile.sprite = photoProfiles[UnityEngine.Random.Range(0 , photoProfiles.Length)];
            ShowReaction(Reaction.NewPlayer);
        }
    }

    private void Start()
    {
        GDi.Request(this);
    }

    public void Initialize()
    {
        CultureInfo cultureInfo = new CultureInfo("de-DE"); // German culture uses dot as thousands separator
        string formattedNumber = pokerPlayer.Money.ToString("N0", cultureInfo);
        moneyText.SetText("$ "+ formattedNumber);
    }

    public void ShowReaction(Reaction reaction)
    {
        emoticon.DOKill();
        emoticon.sprite = this.reaction[(int)reaction];
        emoticon.DOFade(1, 0.5f).OnComplete(() => 
        {
            emoticon.DOFade(0, 1f).SetDelay(1);
        });
    }
}
