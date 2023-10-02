using Gtion.Plugin.DI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pker.UI
{
    public class PlayerControl : MonoBehaviour, IInjectCallback
    {
        [GInject]
        PokerGameManager pokerManager;

        [SerializeField]
        int playerId;
        [SerializeField]
        PlayerDeckView deckHolder;
        [SerializeField]
        PossibleCombinationsView combinationsView;

        public PokerGameManager PokerManager => pokerManager;
        public PokerPlayer MyPlayer => pokerManager.PokerPlayers[playerId];
        public int PlayerId => playerId;

        public bool IsDependencyReady { get; set; }

        private void Start()
        {
            GDi.Request(this);
        }

        public void OnDependencyReady()
        {
            pokerManager.OnNewCardShared.AddListener(Init);
            GDi.Register(this);
        }

        private void Init() 
        {
            deckHolder.Initialize(MyPlayer.Cards);
        }
    }
}