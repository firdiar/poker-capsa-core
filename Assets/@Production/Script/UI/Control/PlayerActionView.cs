using Gtion.Plugin.DI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pker.UI
{
    public class PlayerActionView : MonoBehaviour, IInjectCallback
    {
        [SerializeField]
        Button skipButton;
        [SerializeField]
        Button playButton;

        [GInject]
        PlayerControl playerControl;
        [GInject]
        PlayerDeckView deckView;

        public bool IsDependencyReady { get; set; }
        public bool IsMyTurn { get; private set; }

        public void OnDependencyReady()
        {
            playButton.onClick.AddListener(PlayCard);
            skipButton.onClick.AddListener(SkipAction);

            playerControl.PokerManager.OnTurnChange.AddListener(OnTurnChange);
            OnTurnChange(playerControl.PokerManager.Turn);
        }

        public void OnTurnChange(int turn) 
        {
            IsMyTurn = playerControl.PlayerId == turn;
            if (IsMyTurn)
            {
                skipButton.interactable = true;
                SetActivePlayButton(deckView.ActiveCombi.IsHigherThan(playerControl.PokerManager.LastCard));
            }
            else
            {
                skipButton.interactable = false;
                SetActivePlayButton(false);
            }
        }

        public void SetActivePlayButton(bool isActive) 
        {
            if (!IsMyTurn) return;

            playButton.interactable = isActive;
        }

        private void PlayCard() 
        {
            if (!deckView.ActiveCombi.IsHigherThan(playerControl.PokerManager.LastCard))
            {
                SetActivePlayButton(false);
                Debug.LogError("Current Active Combination is invalid");
                return;
            }

            if (!IsMyTurn) return;

            if (playerControl.PokerManager.PlayCard(deckView.ActiveCombi))
            {
                deckView.RemoveSelectedCard();
            }
        }

        private void SkipAction()
        {
            if (!IsMyTurn) return;

            playerControl.PokerManager.NextTurn();
        }
    }
}
