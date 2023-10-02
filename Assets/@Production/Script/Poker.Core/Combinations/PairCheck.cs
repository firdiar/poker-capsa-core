using System.Net.Http;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pker.Combination
{
    public struct PairCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //because the worst possible scenario player got 4 of kind 3 times, if we got 4 of kind, it'll be 3 Pair 
            //thus 13/4 -> 3.25 * 3 -> 9.75 -> 9
            return (byte) Mathf.FloorToInt((cardsInHand / 4.0f) * 3);
        }

        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte(); //high card always true as long as the value isn't true
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            //same 2 times
            return CombinationHelper.IsSameNumber(cards, 2);
        }

        public JobHandle GetAllCombinations(NativeArray<Card> cards, byte startIndex, NativeArray<CardCombination> combinations, JobHandle dependency)
        {
            FindAllJob job = new FindAllJob()
            {
                Cards = cards,
                Combinations = combinations,
                StartIndex = startIndex
            };

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
                byte actualIndex = StartIndex;
                for(byte i = 0; i < Cards.Length-1; i++) 
                {
                    if (Cards[i].Number == Cards[i+1].Number) 
                    {
                        var newCombination = new CardCombination()
                        {
                            Combination = PokerCombination.Pair
                        };
                        newCombination.SetCard(0, Cards[i]);
                        newCombination.SetCard(1, Cards[i+1]);
                        Combinations[actualIndex] = newCombination;
                        actualIndex++;
                    }
                }
            }
        }
    }
}
