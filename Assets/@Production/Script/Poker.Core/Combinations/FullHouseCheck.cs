using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pker.Combination
{
    public struct FullHouseCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //because the worst possible scenario player got 1 ThreeOfKind and 5 Pair
            //thus 1 three of kind can pair to all 5 pair -> 5

            byte leftCard = (byte)(Mathf.Max(0 ,cardsInHand - 3) / 2);
            return (byte)leftCard;
        }
        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte();
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            if (CombinationHelper.IsSameNumber(cards, 3))
            {
                return CombinationHelper.IsSameNumber(cards, 2, 3);
            }
            else if (CombinationHelper.IsSameNumber(cards, 2))
            {
                return CombinationHelper.IsSameNumber(cards, 3, 2);
            }
            else
            {
                return false;
            }
        }

        public JobHandle GetAllCombinations(NativeArray<Card> cards, byte startIndex, NativeArray<CardCombination> combinations, JobHandle dependency)
        {
            FindAllJob job = new FindAllJob()
            {
                Cards = cards,
                Combinations = combinations,
                StartIndex = startIndex
            };

            //full house has dependency on three of kind and pair
            return job.Schedule(dependency);
        }

        [BurstCompile]
        public struct FindAllJob : IJob
        {
            [ReadOnly]
            public NativeArray<Card> Cards;

            [NativeDisableContainerSafetyRestriction]
            public NativeArray<CardCombination> Combinations;
            public byte StartIndex;

            public void Execute()
            {
                NativeList<CardCombination> threeOfKind = new NativeList<CardCombination>(4 , Allocator.Temp);
                NativeList<CardCombination> pair = new NativeList<CardCombination>(9 , Allocator.Temp);

                //get all possible Fullhouse
                for (byte i = 0; i < Combinations.Length; i++)
                {
                    var combType = Combinations[i].Combination;
                    if (combType == PokerCombination.ThreeOfAKind)
                    {
                        threeOfKind.Add( Combinations[i]);
                    }
                    else if (combType == PokerCombination.Pair)
                    {
                        pair.Add(Combinations[i]);
                    }
                }

                //remove duplicate of pair inside 3ofKind
                for (byte i = 0; i < threeOfKind.Length; i++)
                {
                    var number = threeOfKind[i].GetCard(0).Number;

                    int pairCount = (pair.Length - 1);//basically if there's 3 of 
                    for (int j = pairCount; j >= 0; j--)
                    {
                        if (pair[j].GetCard(0).Number == number)
                        {
                            pair.RemoveAt(j);//try remove all three of a kind from pair
                        }
                    }
                }

                if (pair.Length > 0 && threeOfKind.Length > 0)
                {
                    //get all combination
                    for (byte i = 0; i < threeOfKind.Length; i++)
                    {
                        for (byte j = 0; j < pair.Length; j++)
                        {
                            Combinations[StartIndex] = GetFullHouse(threeOfKind[i] , pair[j]);
                            StartIndex++;
                        }
                    }
                }
            }

            private CardCombination GetFullHouse(CardCombination threeOfKind, CardCombination pair)
            {
                var newCombi = new CardCombination()
                {
                    Combination = PokerCombination.FullHouse
                };

                newCombi.SetCard(0, threeOfKind.GetCard(0));
                newCombi.SetCard(1, threeOfKind.GetCard(1));
                newCombi.SetCard(2, threeOfKind.GetCard(2));

                newCombi.SetCard(3, pair.GetCard(0));
                newCombi.SetCard(4, pair.GetCard(1));

                return newCombi;
            }
        }
    }
}
