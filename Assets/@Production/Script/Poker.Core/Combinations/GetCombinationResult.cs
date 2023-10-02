using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace Pker.Combination
{
    public struct GetCombinationResult
    {
        public NativeArray<CardCombination> Combinations;
        public int Size;
        
        public void Compact()
        {
            Size = PokerHelper.CompactArray(Combinations);
        }

        public void Dispose()
        {
            Combinations.Dispose();
        }
    }
}