using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pker.Combination
{
    public struct StraightFlushCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //worst case scenario all card are straight, and giving +1 as special case [A,K,Q,J,10,9,8,7,6,5,4,3,2,A]
            if (cardsInHand < 5)
                return 0;

            return (byte)Mathf.Max(0, cardsInHand - 3);
        }

        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte();
        }

        public unsafe bool ValidateCombination(byte* cards)
        {
            if (cards[0].GetNumber() == CardNumber.A) return false;

            return CombinationHelper.IsFlush(cards) && CombinationHelper.IsStraight(cards);
        }

        public JobHandle GetAllCombinations(NativeArray<Card> cards, byte startIndex, NativeArray<CardCombination> combinations, JobHandle dependency)
        {
            FindAllJob job = new FindAllJob()
            {
                Cards = cards,
                Combinations = combinations,
                StartIndex = startIndex
            };

            //depend on straight
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

            public unsafe void Execute()
            {
                bool isFlushExist = false;
                bool isStraightExist = false;
                NativeList<byte> startStraight = new NativeList<byte>(13 , Allocator.Temp);
                for (byte i = 0; i < Combinations.Length; i++)
                {
                    if (Combinations[i].Combination == PokerCombination.Straight)
                    {
                        isStraightExist = true;
                        startStraight.Add((byte)Combinations[i].GetCard(0).Number);
                    }
                    else if (Combinations[i].Combination == PokerCombination.Flush)
                    { 
                        isFlushExist = true;
                    }

                    if (isStraightExist && isFlushExist) break;
                }

                if (!isFlushExist || !isStraightExist) return; //there must be no straight flush

                NativeParallelMultiHashMap<byte, Card> mapCard = new NativeParallelMultiHashMap<byte, Card>(Cards.Length, Allocator.Temp);

                //Getting all number
                for (byte i = 0; i < Cards.Length; i++)
                {
                    mapCard.Add((byte)Cards[i].Number, Cards[i]);
                }



                //registering combinations
                for (byte i = 0; i < startStraight.Length; i++) 
                {
                    CardNumber firstNumber = (CardNumber)startStraight[i];                    
                    for (byte j = 0; j < 4; j++) // CardSymbol total
                    {
                        var newCombi = new CardCombination()
                        {
                            Combination = PokerCombination.StraightFlush
                        };

                        CardNumber number = firstNumber;
                        CardSymbol symbol = (CardSymbol)j;
                        bool isValid = true;
                        for (byte k = 0; k < 5; k++) // straight count
                        {
                            if (!GetCardWithSymbol(mapCard, symbol, number, out Card c))
                            {
                                isValid = false;
                                break;
                            }

                            newCombi.SetCard(k, c);
                            number = CombinationHelper.GetNextStraight(number);
                        }

                        if(isValid) 
                        {
                            Combinations[StartIndex] = newCombi;
                            StartIndex++;
                        }
                    }
                }
            }

            public unsafe bool GetCardWithSymbol(NativeParallelMultiHashMap<byte, Card> mapCard , CardSymbol cardSymbol, CardNumber number, out Card cardResult)
            {
                if (mapCard.TryGetFirstValue((byte)number, out Card card, out var it))
                {
                    do
                    {
                        if (card.Symbol == cardSymbol)
                        {
                            cardResult = card;
                            return true;
                        }
                    }
                    while (mapCard.TryGetNextValue(out card, ref it));
                }

                cardResult = default;
                return false;
            }
        }
    }
}
