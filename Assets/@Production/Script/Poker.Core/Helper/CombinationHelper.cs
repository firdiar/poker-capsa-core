using Pker.Combination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;

namespace Pker.Combination
{
    internal unsafe static class CombinationHelper
    {
        internal static bool ValidateCombination(PokerCombination combination, byte* cards)
        {
            switch (combination)
            {
                case PokerCombination.None:
                    return false;
                case PokerCombination.HighCard:
                    return new HighcardCheck().ValidateCombination(cards);
                case PokerCombination.Pair:
                    return new PairCheck().ValidateCombination(cards);
                case PokerCombination.ThreeOfAKind:
                    return new ThreeOfAKindCheck().ValidateCombination(cards);
                case PokerCombination.Straight:
                    return new StraightCheck().ValidateCombination(cards);
                case PokerCombination.Flush:
                    return new FlushCheck().ValidateCombination(cards);
                case PokerCombination.FullHouse:
                    return new FullHouseCheck().ValidateCombination(cards);
                case PokerCombination.FourOfAKind_1:
                    return new FourOfAKind1Check().ValidateCombination(cards);
                case PokerCombination.StraightFlush:
                    return new StraightFlushCheck().ValidateCombination(cards);
                case PokerCombination.RoyalStraightFlush:
                    return new StraightRoyalFlushCheck().ValidateCombination(cards);
            }

            return false;
        }

        internal static byte GetCombinationValue(PokerCombination combination, byte* cards)
        {
            switch (combination)
            {
                case PokerCombination.None:
                    return 0;
                case PokerCombination.HighCard:
                    return new HighcardCheck().GetCombinationValue(cards);
                case PokerCombination.Pair:
                    return new PairCheck().GetCombinationValue(cards);
                case PokerCombination.ThreeOfAKind:
                    return new ThreeOfAKindCheck().GetCombinationValue(cards);
                case PokerCombination.Straight:
                    return new StraightCheck().GetCombinationValue(cards);
                case PokerCombination.Flush:
                    return new FlushCheck().GetCombinationValue(cards);
                case PokerCombination.FullHouse:
                    return new FullHouseCheck().GetCombinationValue(cards);
                case PokerCombination.FourOfAKind_1:
                    return new FourOfAKind1Check().GetCombinationValue(cards);
                case PokerCombination.StraightFlush:
                    return new StraightFlushCheck().GetCombinationValue(cards);
                case PokerCombination.RoyalStraightFlush:
                    return new StraightRoyalFlushCheck().GetCombinationValue(cards);
            }

            return 0;
        }

        internal static unsafe bool IsSameNumber(byte* cards, byte count, byte start = 0)
        {
            CardNumber number = cards[start].GetNumber();
            if (number == CardNumber.None) return false;

            int limit = start + count;
            for (int i = start+1; i < limit; i++)
            {
                if (number != cards[i].GetNumber()) return false;
            }

            return true;
        }

        internal static unsafe bool IsFlush(byte* cards)
        {
            CardSymbol symbol = cards[0].GetSymbol();
            for (int i = 1; i < 5; i++)
            {
                if (symbol != cards[i].GetSymbol()) return false;
            }

            return true;
        }

        internal static unsafe bool IsStraight(byte* cards)
        {
            var lastNumber = cards[0].GetNumber();
            if ((byte)lastNumber < 5 || lastNumber == CardNumber.Two)
            {
                return false;//no straight for number less than 5, and two are not considered poker here
            }

            for (int i = 1; i < 5; i++)
            {
                var newNumber = cards[i].GetNumber();
                if (!IsNextStraight(lastNumber, newNumber, i)) return false;

                lastNumber = newNumber;
            }

            return true;
        }

        internal static unsafe bool IsNextStraight(CardNumber prevNumber, CardNumber currentNumber, int straightIndex)
        {
            if (prevNumber == CardNumber.None) return true;

            if (prevNumber == CardNumber.Three)
            {
                if (straightIndex < 3) return false; //[0,1,2,3,4] only for index 3 and 4 [6,5,4,3,2] or [5,4,3,2,A]

                return currentNumber == CardNumber.Two;
            }
            else if (prevNumber == CardNumber.Two)
            {
                if (straightIndex < 4) return false;//[0,1,2,3,4] only for index 4 [5,4,3,2,A]

                return currentNumber == CardNumber.A;
            }
            else
            {
                return ((byte)prevNumber - (byte)currentNumber) == 1; //it must exactly 1
            }
        }

        internal static CardNumber GetNextStraight(CardNumber prevNumber)
        {
            if (prevNumber == CardNumber.Three)
            {
                return CardNumber.Two;
            }
            else if (prevNumber == CardNumber.Two)
            {
                return CardNumber.A;
            }
            else
            {
                return (CardNumber)((byte)prevNumber - 1);
            }
        }
    }
}
