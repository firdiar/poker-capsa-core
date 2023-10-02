using Gtion.Plugin.DI;
using NaughtyAttributes;
using Pker;
using Pker.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class TableCardView : MonoBehaviour, IInjectCallback
{
    [SerializeField]
    Transform parentHolder;
    [SerializeField]
    CardHolder prefab;

    [SerializeField]
    TextMeshProUGUI combinationName;

    [GInject]
    PokerGameManager pokerGameManager;

    public CardCombination dummy;

    List<CardHolder> caches = new List<CardHolder>();

    public IObjectPool<CardHolder> pool;

    public bool IsDependencyReady { get; set; }

    private void Start()
    {
        pool = new DefaultObjectPool<CardHolder>(prefab, parentHolder);
        GDi.Request(this);
    }

    public void OnDependencyReady()
    {
        pokerGameManager.OnCardPlayed.AddListener(ShowCardsAsync);
    }

    [Button]
    private void ShowDummy()
    {
        ShowCardsAsync(dummy);
    }

    public async void ShowCardsAsync(CardCombination cardCombination)
    {
        Clear();
        await Task.Delay(700);
        if (cardCombination.Combination == PokerCombination.None)
        {
            combinationName.SetText("-");
        }
        else
        {
            combinationName.SetText(cardCombination.Combination.ToString());
        }

        int cardCount = cardCombination.Combination.GetCardCount();
        for(byte i = 0; i < cardCount; i++) 
        {
            var poolObj = pool.Get();
            poolObj.transform.SetAsLastSibling();
            poolObj.Init(cardCombination.GetCard(i) , 0.15f * i);

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
}
