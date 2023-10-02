using Pker.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Pker
{
    [System.Serializable]
    public class PokerPlayer
    {
        PokerGameManager gameManager;

        [SerializeField]
        long money;
        public long Money { get => money; set => money = value; }

        [SerializeField]
        List<Card> cards = new List<Card>();
        public IReadOnlyList<Card> Cards => cards;

        [SerializeField]
        List<CardCombination> availableCombination = new List<CardCombination>(64);

        public IReadOnlyList<CardCombination> CardCombination => availableCombination;

        public UnityEvent OnCombinationUpdated { get; set; } = new UnityEvent();

        public PokerPlayer(PokerGameManager gameManager , long money) 
        {
            this.money = money;
            this.gameManager = gameManager;

        }

        public void UpdateCard(NativeArray<Card> initCards) 
        {
            foreach (var card in initCards)
            {
                cards.Add(card);
            }

            cards = cards.OrderByDescending(card => card.Number).ThenBy(card => card.Symbol).ToList();
            availableCombination.Clear();
        }

        public void UpdatePossibleCombination() 
        {
            availableCombination.Clear();
            NativeArray<Card> allCard = new NativeArray<Card>(cards.ToArray(), Allocator.TempJob);
            var jobHandle = PokerHelper.GetAllCombinations(allCard, out var result);
            jobHandle.Complete();

            SetCombinations(result);
            allCard.Dispose();
        }

        public void SetCombinations(GetCombinationResult result)
        {
            availableCombination.Clear();
            result.Compact();
            for (int i = 0; i < result.Size; i++)
            {
                availableCombination.Add(result.Combinations[i]);
            }
            result.Dispose();
            OnCombinationUpdated.Invoke();
        }

        internal void UseCard(List<int> indexes) 
        {
            for(int  i = indexes.Count - 1; i >= 0; i--) 
            {
                cards.RemoveAt(indexes[i]);
            }
            UpdatePossibleCombination();
        }

        public bool IsPlayerHasCard(CardCombination combination)
        {
            return IsPlayerHasCard(combination, out var temp);
        }

        public bool IsPlayerHasCard(CardCombination combination, out List<int> indexes)
        {
            indexes = new List<int>();
            int cardCount = combination.Combination.GetCardCount();
            byte lastSearchIdx = 0;
            for(byte i = 0; i < cardCount; i++) 
            {
                var combiCard = combination.GetCard(i);
                bool found = false;
                for (byte j = lastSearchIdx; j < cards.Count; j++) //efficient search
                {
                    if (cards[j].Equals(combiCard))
                    {
                        indexes.Add(j);
                        found = true;
                        break;
                    }
                    else
                    {
                        lastSearchIdx++;
                    }                    
                }

                if (!found) return false;
            }

            return true;
        }
    }
}
