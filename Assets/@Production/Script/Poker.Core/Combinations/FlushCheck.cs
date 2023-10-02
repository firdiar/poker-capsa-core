using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pker.Combination
{
    public struct FlushCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //because the worst possible scenario player got all 13 same symbold, but only 5 are possible to used as flush
            //thus 13 - 4 -> 9
            return (byte) Mathf.Max( 0 , cardsInHand - 4);
        }
        public unsafe byte GetCombinationValue(byte* cards)
        {
            byte total = 0;
            for (int i = 1; i < 5; i++)
            {
                total += cards[i].GetNumberAsByte();
            }
            return total;
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            return CombinationHelper.IsFlush(cards) && !CombinationHelper.IsStraight(cards);
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
            const byte Four = 4;
            const byte Five = 5;

            [ReadOnly]
            public NativeArray<Card> Cards;

            [NativeDisableContainerSafetyRestriction]
            public NativeArray<CardCombination> Combinations;
            public byte StartIndex;

            public void Execute()
            {
                NativeArray<byte> symbolCount = new NativeArray<byte>(4, Allocator.Temp);
                NativeArray<byte> indexCards = new NativeArray<byte>(20, Allocator.Temp);
                
                for (byte i = 0; i < Cards.Length; i++)
                {
                    byte symbolId = (byte)Cards[i].Symbol;
                    AddLastItem(indexCards, symbolId, symbolCount[symbolId], i);

                    symbolCount[symbolId]++;
                    if (symbolCount[symbolId] >= 5)
                    {
                        Combinations[StartIndex] = GetCombination(indexCards, symbolId);
                        RemoveFirstItem(indexCards, symbolId);
                        StartIndex++;
                    }
                }
            }

            private CardCombination GetCombination(NativeArray<byte> array, byte symbolId)
            {
                var newCombi = new CardCombination()
                {
                    Combination = PokerCombination.Flush
                };

                byte startIndex = (byte)(symbolId * Five);
                for (byte j = 0; j < 5; j++)
                {
                    newCombi.SetCard(j, Cards[array[startIndex]]);
                    startIndex++;
                }

                return newCombi;
            }

            private void AddLastItem(NativeArray<byte> array, byte symbolId, byte symbolCount, byte value)
            {
                int startIndex = symbolId * 5;
                byte index = Math.Min(symbolCount, Four);
                array[startIndex + index] = value;
            }

            private void RemoveFirstItem(NativeArray<byte> array , byte symbolId)
            {
                //move all 4 index forward by 1
                int startIndex = symbolId * 5;
                array[startIndex] = array[startIndex + 1];
                array[startIndex+1] = array[startIndex + 2];
                array[startIndex+2] = array[startIndex + 3];
                array[startIndex+3] = array[startIndex + 4];
                array[startIndex+4] = 0;
            }
        }
    }
}
