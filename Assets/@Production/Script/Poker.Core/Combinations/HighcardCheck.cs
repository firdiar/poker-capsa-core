using static Pker.Combination.PairCheck;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Pker.Combination
{
    public struct HighcardCheck : ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand)
        {
            //All cards in hand are possible to be highcard
            return cardsInHand;
        }
        public unsafe byte GetCombinationValue(byte* cards)
        {
            return cards[0].GetNumberAsByte(); //high card always true as long as the value isn't true
        }
        public unsafe bool ValidateCombination(byte* cards)
        {
            return cards[0].GetNumber() != CardNumber.None;
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
                for (byte i = 0; i < Cards.Length; i++)
                {
                    var newCombination = new CardCombination()
                    {
                        Combination = PokerCombination.HighCard
                    };
                    newCombination.SetCard(0, Cards[i]);
                    Combinations[StartIndex] = newCombination;
                    StartIndex++;
                }
            }
        }
    }
}
