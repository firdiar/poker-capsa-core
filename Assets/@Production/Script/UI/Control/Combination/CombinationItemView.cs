using Gtion.Plugin.DI;
using NaughtyAttributes;
using Pker;
using Pker.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombinationItemView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Button button;

    [SerializeField]
    [ReadOnly]
    PokerCombination combination;

    
    [GInject]
    PlayerDeckView deckView;

    List<CardCombination> combinations;

    int index = 0;

    private void Start()
    {
        button.onClick.AddListener(SelectCombination);
        GDi.Request(this);
    }

    public void Initialize(PokerCombination combination, List<CardCombination> combinations)
    {
        this.combination = combination;
        this.combinations = combinations;
        text.SetText(combination.ToString());
        index = 0;
    }

    public void SelectCombination()
    {
        var comb = combinations[index];
        deckView.SelectCard(comb);

        index = (index+1)% combinations.Count;
    }
}
