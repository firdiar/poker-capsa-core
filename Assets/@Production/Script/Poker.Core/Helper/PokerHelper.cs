using Pker.Combination;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEditor.Experimental.GraphView;

namespace Pker
{
    public static class PokerHelper
    {
        public const byte FirstDrawAmount = 13;

        public static string AsString(this CardNumber cardNumber)
        {
            switch (cardNumber)
            {
                case CardNumber.None:
                    return "<None>";
                case CardNumber.Three:
                    return "3";
                case CardNumber.Four:
                    return "4";
                case CardNumber.Five:
                    return "5";
                case CardNumber.Six:
                    return "6";
                case CardNumber.Seven:
                    return "7";
                case CardNumber.Eight:
                    return "8";
                case CardNumber.Nine:
                    return "9";
                case CardNumber.Ten:
                    return "10";
                case CardNumber.J:
                    return "J";
                case CardNumber.Q:
                    return "Q";
                case CardNumber.K:
                    return "K";
                case CardNumber.A:
                    return "A";
                case CardNumber.Two:
                    return "2";
            }
            return "<None>";
        }

        public static unsafe bool GetCombination(List<Card> cards, out CardCombination cardCombination) //assumed the array is ordered
        {
            if (cards.Count == 0)
            {
                cardCombination = default;
                return false;
            }

            HashSet<CardNumber> cardNumbers = new HashSet<CardNumber>();
            HashSet<CardSymbol> cardSymbols = new HashSet<CardSymbol>();

            bool isContains2 = false;
            bool isContainsAce = false;
            bool isContainDuplicateNumber = false;
            foreach (var card in cards)
            {
                if (card.Number == CardNumber.Two)
                {
                    isContains2 = true;
                }
                else if (card.Number == CardNumber.A)
                {
                    isContainsAce = true;
                }

                if (!cardNumbers.Add(card.Number))
                {
                    isContainDuplicateNumber = true;
                }
                cardSymbols.Add(card.Symbol);
            }

            NativeArray<byte> cardNative = new NativeArray<byte>(5, Allocator.Temp);
            bool isStraigPossible = cards.Count == 5 && !isContainDuplicateNumber;
            byte* cardNativeByte;
            if (isStraigPossible) //checking straight
            {
                if (isContains2)
                {
                    if (isContainsAce)
                    {
                        for (int i = 0; i < 5; i++) cardNative[i] = cards[(i + 2) % 5].ToByte();
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++) cardNative[i] = cards[(i + 1) % 5].ToByte();
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++) cardNative[i] = cards[i].ToByte();
                }

                cardNativeByte = (byte*)cardNative.GetUnsafePtr();
                if (CombinationHelper.ValidateCombination(PokerCombination.RoyalStraightFlush, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.RoyalStraightFlush
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
                else if (CombinationHelper.ValidateCombination(PokerCombination.StraightFlush, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.StraightFlush
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
                else if (cardSymbols.Count != 1 && CombinationHelper.ValidateCombination(PokerCombination.Straight, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.Straight
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
            }

            for (int i = 0; i < cards.Count; i++) cardNative[i] = cards[i].ToByte();

            cardNativeByte = (byte*)cardNative.GetUnsafePtr();
            if (cards.Count == 5)
            {
                if (CombinationHelper.ValidateCombination(PokerCombination.FourOfAKind_1, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.FourOfAKind_1
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
                else if (CombinationHelper.ValidateCombination(PokerCombination.FullHouse, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.FullHouse
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
                else if (cardSymbols.Count == 1 && CombinationHelper.ValidateCombination(PokerCombination.Flush, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.Flush
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
            }
            else if (cards.Count == 3)
            {
                if (CombinationHelper.ValidateCombination(PokerCombination.ThreeOfAKind, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.ThreeOfAKind
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
            }
            else if (cards.Count == 2)
            {
                if (CombinationHelper.ValidateCombination(PokerCombination.Pair, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.Pair
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
            }
            else if (cards.Count == 1) 
            {
                if (CombinationHelper.ValidateCombination(PokerCombination.HighCard, cardNativeByte))
                {
                    cardCombination = new CardCombination()
                    {
                        Combination = PokerCombination.HighCard
                    };
                    cardCombination.SetCard(cardNativeByte);
                    return true;
                }
            }

            cardCombination = default;
            return false;
        }

        /// <summary>
        /// Get All Combination that is possible in player hands, but make sure to send sorted cards
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        internal static JobHandle GetAllCombinations(NativeArray<Card> cards, out GetCombinationResult result)
        {
            result = new GetCombinationResult();
            NativeArray<JobHandle> allJobs = new NativeArray<JobHandle>(9 , Allocator.Temp);
            NativeArray<CardCombination> resultCombination = new NativeArray<CardCombination>(66, Allocator.TempJob);
            result.Combinations = resultCombination;

            byte startIndex = 0;
            byte cardsInHand = (byte)cards.Length;

            var pair = new PairCheck();
            var pairHandle = pair.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += pair.GetMaxCombination(cardsInHand);
            allJobs[0] = pairHandle;

            var three = new ThreeOfAKindCheck();
            var threeHandle = three.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += three.GetMaxCombination(cardsInHand);
            allJobs[1] = threeHandle;

            var four = new FourOfAKind1Check();
            var fourHandle = four.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += four.GetMaxCombination(cardsInHand);
            allJobs[2] = fourHandle;

            var flush = new FlushCheck();
            var flushHandle = flush.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += flush.GetMaxCombination(cardsInHand);
            allJobs[3] = flushHandle;

            var straight = new StraightCheck();
            var straightHandle = straight.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += straight.GetMaxCombination(cardsInHand);
            allJobs[4] = straightHandle;

            var fullhouse = new FullHouseCheck();
            var fullhouseHandle = fullhouse.GetAllCombinations(cards, startIndex, resultCombination, JobHandle.CombineDependencies(pairHandle , threeHandle));
            startIndex += fullhouse.GetMaxCombination(cardsInHand);
            allJobs[5] = fullhouseHandle;

            var straightflush = new StraightFlushCheck();
            var straightflushHandle = straightflush.GetAllCombinations(cards, startIndex, resultCombination, JobHandle.CombineDependencies(flushHandle, straightHandle));
            startIndex += straightflush.GetMaxCombination(cardsInHand);
            allJobs[6] = straightflushHandle;

            var royalstraightflush = new StraightRoyalFlushCheck();
            var royalstraightflushHandle = royalstraightflush.GetAllCombinations(cards, startIndex, resultCombination, JobHandle.CombineDependencies(flushHandle, straightHandle));
            startIndex += royalstraightflush.GetMaxCombination(cardsInHand);
            allJobs[7] = royalstraightflushHandle;

            var hc = new HighcardCheck();
            var hcHandle = hc.GetAllCombinations(cards, startIndex, resultCombination, default);
            startIndex += hc.GetMaxCombination(cardsInHand);
            allJobs[8] = hcHandle;

            //UnityEngine.Debug.Log("TotalIndex : "+startIndex);
            return JobHandle.CombineDependencies(allJobs);
        }

        public static int CompactArray(NativeArray<CardCombination> array)
        {
            int writeIndex = 0;

            for (int readIndex = 0; readIndex < array.Length; readIndex++)
            {
                // Consider non-zero values as items
                if (array[readIndex].Combination != PokerCombination.None)
                {
                    array[writeIndex] = array[readIndex];
                    writeIndex++;
                }
            }

            // Fill the rest of the array with zeros (considered as empty)
            for (int i = writeIndex; i < array.Length; i++)
            {
                array[i] = default;
            }

            return writeIndex;
        }

        public static NativeArray<Card> GetFullDeck(Allocator allocator) 
        {
            NativeArray<Card> deck = new NativeArray<Card>(52 , allocator);
            int loop = 0;
            foreach (CardNumber number in Enum.GetValues(typeof(CardNumber)))
            {
                if (number == CardNumber.None) continue;

                foreach (CardSymbol symbol in Enum.GetValues(typeof(CardSymbol)))
                {
                    deck[loop] = new Card(symbol, number);
                    loop++;
                }
            }

            return deck;
        }

        public static byte GetNumberAsByte(this byte b)
        {
            return Card.GetNumberAsByte(b);
        }
        public static CardNumber GetNumber(this byte b)
        {
            return Card.GetNumber(b);
        }

        public static CardSymbol GetSymbol(this byte b)
        {
            return Card.GetSymbol(b);
        }

        public static bool IsHigherThan(this CardCombination newCombination, CardCombination oldCombination)
        {
            if(!newCombination.IsValid()) return false;

            if(!oldCombination.IsValid()) return true; //basically if the old was empty then allow any valid combination

            //card count must match
            byte cardCount = newCombination.Combination.GetCardCount();
            byte cardCount2 = oldCombination.Combination.GetCardCount();
            if(cardCount != cardCount2) return false;

            if (newCombination.Combination != oldCombination.Combination)
            {
                if (newCombination.Combination > oldCombination.Combination)
                {
                    return true;
                }
                else //this must means newCombination < oldCombination
                {
                    return false;
                }
            }

            var newVal = newCombination.GetValue();
            var oldVal = oldCombination.GetValue();
            if (newVal > oldVal)
            {
                return true;
            }
            else if (newVal < oldVal) 
            {
                return false;
            }

            if (newCombination.GetSymbol() >= oldCombination.GetSymbol())
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        public static bool IsLowerThan(this CardCombination newCombination, CardCombination oldCombination)
        {
            if (!newCombination.IsValid()) return false;

            if (!oldCombination.IsValid()) return true; //basically if the old was empty then allow any valid combination

            //card count must match
            byte cardCount = newCombination.Combination.GetCardCount();
            byte cardCount2 = oldCombination.Combination.GetCardCount();
            if (cardCount != cardCount2) return false;

            return !IsHigherThan(newCombination , oldCombination);
        }

        public static byte GetCardCount(this PokerCombination cardCombination)
        {
            switch (cardCombination)
            {
                case PokerCombination.None:
                    return 0;
                case PokerCombination.HighCard:
                    return 1;
                case PokerCombination.Pair:
                    return 2;
                case PokerCombination.ThreeOfAKind:
                    return 3;
            }

            return 5; //the other type are 5 card combination
        }
    }
}
