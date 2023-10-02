using Gtion.Plugin.DI;
using Pker;
using Pker.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class PossibleCombinationsView : MonoBehaviour, IInjectCallback
{
    [GInject]
    PlayerControl playerControl;

    [SerializeField]
    Transform parentHolder;
    [SerializeField]
    CombinationItemView itemView;

    DefaultObjectPool<CombinationItemView> pool;
    List<CombinationItemView> cache = new List<CombinationItemView>();
    public bool IsDependencyReady { get; set; }

    private void Start()
    {
        pool = new DefaultObjectPool<CombinationItemView>(itemView, parentHolder);
    }
    public async void OnDependencyReady()
    {
        await playerControl.PokerManager.WaitUntilGameStart();

        playerControl.MyPlayer.OnCombinationUpdated.AddListener(UpdateList);
        UpdateList();
    }

    private void UpdateList() 
    {
        foreach (var c in cache)
        { 
            pool.Release(c);
        }
        cache.Clear();

        Dictionary<PokerCombination, List<CardCombination>> comb = new Dictionary<PokerCombination, List<CardCombination>>();
        foreach (var item in playerControl.MyPlayer.CardCombination)
        {
            if (comb.TryGetValue(item.Combination, out var list))
            {
                list.Add(item);
            }
            else
            { 
                comb.Add(item.Combination , new List<CardCombination>(4) { item });
            }
        }

        var orderedComb = comb.Keys.ToList();
        orderedComb.Sort((a, b) => (byte)b - (byte)a);

        foreach (var item in orderedComb)
        {
            var itemView = pool.Get();
            itemView.transform.SetAsLastSibling();
            itemView.Initialize(item , comb[item]);
            cache.Add(itemView);
        }
    }
}
