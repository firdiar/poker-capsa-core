using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pker.Combination
{
    public struct FourOfAKind1Check : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //because the worst possible scenario player got 4 of kind 3 times
            //thus 13/4 -> 3
            return (byte)(cardsInHand / 4);
        }
        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte();
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            //same 4 times
            if (CombinationHelper.IsSameNumber(cards, 4))
            {
                return cards[4].GetNumber() != CardNumber.None;
            }
            else if (CombinationHelper.IsSameNumber(cards, 4, 1))
            {
                return cards[0].GetNumber() != CardNumber.None;
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
                var max = Cards.Length - 3;
                for (byte i = 0; i < max; i++)
                {
                    bool isFourOfKind = Cards[i].Number == Cards[i + 1].Number && 
                                        Cards[i].Number == Cards[i + 2].Number && 
                                        Cards[i].Number == Cards[i + 3].Number;
                    if (isFourOfKind)
                    {
                        var lowestCard = GetLowestCard(Cards[i].Number);
                        if (lowestCard.Number == CardNumber.None) continue; //not found

                        var newCombination = new CardCombination()
                        {
                            Combination = PokerCombination.Pair
                        };
                        newCombination.SetCard(0, Cards[i]);
                        newCombination.SetCard(1, Cards[i + 1]);
                        newCombination.SetCard(2, Cards[i + 2]);
                        newCombination.SetCard(3, Cards[i + 3]);
                        newCombination.SetCard(4, lowestCard);

                        Combinations[StartIndex] = newCombination;
                        StartIndex++;
                    }
                }
            }

            public Card GetLowestCard(CardNumber excludeNumber)
            {
                for (var i = Cards.Length - 1; i >= 0; i--)
                {
                    if (Cards[i].Number != excludeNumber)
                    {
                        return Cards[i];
                    }
                }

                return default;
            }
        }
    }
}
