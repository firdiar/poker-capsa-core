using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Profiling;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pker.Combination
{
    public struct StraightCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //worst case scenario all card are straight card-4, and giving +1 as special case [A,K,Q,J,10,9,8,7,6,5,4,3,2,A]
            if (cardsInHand < 5)
                return 0;

            return (byte)Mathf.Max(0 , cardsInHand-3);
        }

        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte();
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            return CombinationHelper.IsStraight(cards);
        }

        public JobHandle GetAllCombinations(NativeArray<Card> cards, byte startIndex, NativeArray<CardCombination> combinations, JobHandle dependency)
        {
            FindAllJob job = new FindAllJob()
            {
                Marker = myMarker,
                Marker2 = myMarker2,
                Cards = cards,
                Combinations = combinations,
                StartIndex = startIndex
            };

            return job.Schedule(dependency);
        }
        static readonly ProfilerMarker myMarker = new ProfilerMarker("StraightCheck Job");
        static readonly ProfilerMarker myMarker2 = new ProfilerMarker("StraightCheck Job2");

        [BurstCompile]
        public struct FindAllJob : IJob
        {
            [ReadOnly]
            public NativeArray<Card> Cards;

            [NativeDisableContainerSafetyRestriction]
            public NativeArray<CardCombination> Combinations;
            public byte StartIndex;

            public ProfilerMarker Marker;
            public ProfilerMarker Marker2;

            public unsafe void Execute()
            {
                
                byte extraLen = 0;
                CardNumber lastCard = CardNumber.None;
                NativeList<CardNumber> extraNumber = new NativeList<CardNumber>(2, Allocator.Temp);
                NativeList<CardNumber> allNumber = new NativeList<CardNumber>(Cards.Length + 4, Allocator.Temp);
                NativeParallelHashMap<byte, Card> mapCard = new NativeParallelHashMap<byte, Card>(Cards.Length , Allocator.Temp);

                Marker.Begin();
                //Getting all number
                for (byte i = 0; i < Cards.Length + extraLen; i++)
                {
                    if (i >= Cards.Length)
                    {
                        var extraIdx = i - Cards.Length;
                        CardNumber number = extraNumber[extraIdx];
                        if (lastCard == number) continue;

                        lastCard = number;
                        allNumber.Add(number);
                    }
                    else
                    {
                        var number = Cards[i].Number;
                        if (lastCard == number) continue;


                        mapCard.Add((byte)number, Cards[i]);
                        lastCard = number;
                        if (number == CardNumber.Two)
                        {
                            extraLen++;
                            extraNumber.Add(CardNumber.Two);
                            continue;
                        }

                        if (number == CardNumber.A)
                        {
                            extraLen++;
                            extraNumber.Add(CardNumber.A);
                        }
                        
                        allNumber.Add(number);
                    }
                }
                Marker.End();

                Marker2.Begin();
                //getting all straight from number
                NativeArray<byte> straightArr = new NativeArray<byte>(5, Allocator.Temp);
                for (byte i = 0; i < allNumber.Length-4; i++)
                {
                    for (byte j = 0; j < 5; j++)
                    {
                        straightArr[j] = (byte)allNumber[i + j];
                    }

                    byte* bytePtr = (byte*)straightArr.GetUnsafePtr();
                    if (CombinationHelper.IsStraight(bytePtr))
                    {
                        RegisterNewCombi(mapCard, bytePtr);
                    }
                }
                Marker2.End();
            }

            private unsafe void RegisterNewCombi(NativeParallelHashMap<byte, Card> mapCard , byte* byteCard)
            {
                var newCombi = new CardCombination() { Combination = PokerCombination.Straight };
                bool isValid = true;
                for (byte j = 0; j < 5; j++)
                {
                    if (mapCard.TryGetValue(byteCard[j], out Card item))
                    {
                        newCombi.SetCard(j, item);
                    }
                    else
                    {
                        isValid = false;//this shouldn't be possible
                        Debug.LogError("this shouldn't be possible, the next straight card not found?!");
                        break;
                    }
                }

                if (isValid)
                {
                    Combinations[StartIndex] = newCombi;
                    StartIndex++;
                }
            }
        }        
    }
}
