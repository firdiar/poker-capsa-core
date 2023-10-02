using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;

namespace Pker.Combination
{
    public interface ICombinationChecker
    {
        public byte GetMaxCombination(byte cardsInHand);
        public unsafe byte GetCombinationValue(byte* cards);
        public unsafe bool ValidateCombination(byte* cards);
        public JobHandle GetAllCombinations(NativeArray<Card> cards, byte startIndex, NativeArray<CardCombination> combinations, JobHandle dependency);
    }
}
