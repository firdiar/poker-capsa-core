using Gtion.Plugin.DI;
using Pker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIController : MonoBehaviour, IInjectCallback
{
    [SerializeField]
    int playerId;

    [GInject]
    PokerGameManager pokerManager;
    PokerPlayer Player => pokerManager.PokerPlayers[playerId];
    public bool IsDependencyReady { get; set; }

    public void OnDependencyReady()
    {
        pokerManager.OnTurnChange.AddListener(OnTurnChange);
    }

    private void Start()
    {
        GDi.Request(this);
    }

    private void OnTurnChange(int turn)
    {
        if (turn == playerId)
        {
            Think();
        }
    }

    List<CardCombination> availableCombi =  new List<CardCombination>();
    Dictionary<PokerCombination, List<KeyValuePair<float , CardCombination>>> allweight = new Dictionary<PokerCombination, List<KeyValuePair<float, CardCombination>>>();
    Dictionary<PokerCombination, float> totalWeight = new Dictionary<PokerCombination, float>();
    private async void Think() 
    {
        await Task.Delay(500); //initial thinking

        availableCombi.Clear();
        allweight.Clear();
        totalWeight.Clear();
        foreach (var combi in Player.CardCombination)
        {
            if (combi.IsHigherThan(pokerManager.LastCard))
            {
                availableCombi.Add(combi);

                //weighted combination
                var valueWeight = (255 - combi.GetValue()) *4;
                var pair = new KeyValuePair<float, CardCombination>(valueWeight, combi);
                if (allweight.TryGetValue(combi.Combination, out List<KeyValuePair<float, CardCombination>> weightList))
                {
                    weightList.Add(pair);
                    totalWeight[combi.Combination] += valueWeight;
                }
                else
                {
                    allweight.Add(combi.Combination, new List<KeyValuePair<float, CardCombination>>() { pair });
                    totalWeight[combi.Combination] = valueWeight;
                }
            }
        }

        CardCombination finalCombi = default;
        bool isImpossibleToSkip = pokerManager.LastCard.Combination == PokerCombination.None;//impossible to skip None Combination
        if (!isImpossibleToSkip && (availableCombi.Count == 0 || UnityEngine.Random.Range(0, 100f) < 10)) //20% chance to skip
        {
            //skip
            ExecuteAction(finalCombi);
        }
        else
        {
            //select combination
            var randomCombi = UnityEngine.Random.Range(0, allweight.Count);
            PokerCombination selectedCombi = default;
            float totalRandom = 0;
            foreach (var pair in totalWeight)
            {
                if (randomCombi <= 0)
                {
                    selectedCombi = pair.Key;
                    totalRandom = pair.Value;
                    break;
                }

                randomCombi--;
            }

            //select combi randomly
            var randm = UnityEngine.Random.Range(0, totalRandom);
            foreach (var pair in allweight[selectedCombi])
            {
                randm -= pair.Key;
                if (randm <= 0)
                {
                    finalCombi = pair.Value;
                    break;
                }
            }

            ExecuteAction(finalCombi);
        }        
    }

    private async void ExecuteAction(CardCombination lowestCombi)
    {
        if (lowestCombi.Combination == PokerCombination.None)
        {
            await Task.Delay(1500); //initial thinking
            if (pokerManager.Turn == playerId)
            {
                pokerManager.NextTurn();
            }
            else
            {
                Debug.LogError(playerId + " - Invalid Action when it's not user turn");
            }
            Debug.Log(playerId + " decided to skip ");
            return;
        }

        await Task.Delay(3000); //initial thinking

        if (pokerManager.Turn == playerId)
        {
            if (!pokerManager.PlayCard(lowestCombi))
            {
                Debug.Log(playerId + " decided to play but failed : " + lowestCombi.Combination);
                pokerManager.NextTurn();
            }
            else
            {
                Debug.Log(playerId + " decided to play : " + lowestCombi.Combination);
            }
        }
        else
        {
            Debug.LogError(playerId + " -Invalid Action when it's not user turn");
        }
    }
}
