using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pker.Combination
{
    public struct ThreeOfAKindCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //because the worst possible scenario player got 4 of kind 3 times, if we got 4 of kind, it'll be 2 ThreeOfKind 
            //thus 13/4 -> 3 * 2 -> 6
            return (byte)(Mathf.FloorToInt((cardsInHand / 4.0f) * 2));
        }
        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte();
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            //same 3 times
            return CombinationHelper.IsSameNumber(cards, 3);
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
                for (byte i = 0; i < Cards.Length - 2; i++)
                {
                    if (Cards[i].Number == Cards[i + 1].Number && Cards[i].Number == Cards[i + 2].Number)
                    {
                        var newCombination = new CardCombination()
                        {
                            Combination = PokerCombination.ThreeOfAKind
                        };
                        newCombination.SetCard(0, Cards[i]);
                        newCombination.SetCard(1, Cards[i + 1]);
                        newCombination.SetCard(2, Cards[i + 2]);
                        Combinations[actualIndex] = newCombination;
                        actualIndex++;
                    }
                }
            }
        }
    }
}
