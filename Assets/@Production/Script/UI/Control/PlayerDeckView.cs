using Gtion.Plugin.DI;
using Pker.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Pker.UI
{
    public class PlayerDeckView : MonoBehaviour
    {

        [SerializeField]
        Transform parentHolder;
        [SerializeField]
        CardHolder cardHolderPrefab;

        List<CardHolder> caches = new List<CardHolder>();

        [GInject]
        PlayerControl playerControl;
        [GInject]
        PlayerActionView playerActionView;

        public IObjectPool<CardHolder> pool;

        public CardCombination ActiveCombi { get; private set; }

        public async void Initialize(IReadOnlyList<Card> allCards) 
        {
            if (pool == null)
            {
                pool = new DefaultObjectPool<CardHolder>(cardHolderPrefab, parentHolder);
            }

            Debug.Log("New Card Init");

            Clear();
            await Task.Delay(1000);
            foreach(var item in allCards) 
            {
                var poolObj = pool.Get();
                var trans = poolObj.transform;
                trans.SetAsLastSibling();
                trans.localScale = Vector3.one;
                poolObj.Init(item);

                caches.Add(poolObj);
            }
        }

        public void Clear()
        {
            foreach (var cache in caches)
            {
                cache.Destroy(pool);
            }
            caches.Clear();
        }

        public void RemoveSelectedCard()
        {
            for (int i = caches.Count - 1; i >= 0; i--)
            {
                if (caches[i].IsSelected)
                {
                    caches[i].Destroy(pool);
                    caches.RemoveAt(i);
                }
            }
        }

        public void SelectCard(CardCombination comb)
        {
            if (!comb.IsValid()) return;

            HashSet<Card> cards = new HashSet<Card>();
            var cardCount = comb.Combination.GetCardCount();
            for (byte i = 0; i < cardCount; i++)
            {
                cards.Add(comb.GetCard(i));
            }

            foreach (var cache in caches)
            {
                if (cards.Contains(cache.Card))
                {
                    cache.SetSelected(true);
                }
                else
                {
                    cache.SetSelected(false);
                }
            }

            ActiveCombi = comb;

            bool isHigherThanLast = ActiveCombi.IsHigherThan(playerControl.PokerManager.LastCard);
            playerActionView.SetActivePlayButton(isHigherThanLast);
        }

        public void SelectCard(Card card) 
        {
            var obj = caches.Find(item => item.Card.Equals(card));
            if (obj == null) return;

            obj.SetSelected(true);
            CheckCards();
        }

        public void UnSelectCard(Card card)
        {
            var obj = caches.Find(item => item.Card.Equals(card));
            if (obj == null) return;

            obj.SetSelected(false);
            CheckCards();
        }

        List<Card> checkCache = new List<Card>();
        private void CheckCards() 
        {
            checkCache.Clear();
            foreach (var card in caches) 
            {
                if(card.IsSelected)
                {
                    checkCache.Add(card.Card);
                }
            }
            
            if (checkCache.Count > 0 && checkCache.Count <= 5)
            {
                ActiveCombi = default;
                var temp = checkCache.OrderByDescending(card => card.Number).ThenBy(card => card.Symbol).ToList();
                bool isvalid = PokerHelper.GetCombination(checkCache, out CardCombination combination);
                ActiveCombi = combination;
                if (isvalid)
                {
                    bool isHigherThanLast = ActiveCombi.IsHigherThan(playerControl.PokerManager.LastCard);
                    playerActionView.SetActivePlayButton(isHigherThanLast);
                }
                else
                {
                    playerActionView.SetActivePlayButton(false);
                }
            }
            else
            {
                playerActionView.SetActivePlayButton(false);
            }

            checkCache.Clear();
        }
    }
}
