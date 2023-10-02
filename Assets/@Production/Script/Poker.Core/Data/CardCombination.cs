using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Pker.Combination;

namespace Pker
{
    [System.Serializable]
    public unsafe struct CardCombination : IEquatable<CardCombination>
    {
        public PokerCombination Combination;
        public fixed byte Cards[5];

        public void SetCard(byte cardIndex, Card card)
        {
            Cards[cardIndex] = card.ToByte();
        }

        public void SetCard(byte cardIndex, byte card)
        {
            Cards[cardIndex] = card;
        }

        public void SetCard(byte* cards)
        {
            for (byte i = 0; i < 5; i++)
            {
                Cards[i] = cards[i];
            }
        }

        public Card GetCard(byte index)
        {
            return new Card(Cards[index]);
        }

        public CardSymbol GetSymbol()
        {
            return Cards[0].GetSymbol();
        }

        public byte GetValue()
        {
            fixed (byte* cardPtr = Cards)
            {
                return CombinationHelper.GetCombinationValue(Combination, cardPtr);
            }
        }

        public bool IsValid() 
        {
            if (Combination == PokerCombination.None || Cards[0].GetNumber() == CardNumber.None)
            {
                return false;
            }

            fixed (byte* cardPtr = Cards)
            {
                return CombinationHelper.ValidateCombination(Combination, cardPtr);
            }
        }

        public bool Equals(CardCombination other)
        {
            //it's not possible for a combination to have same card as firsts
            return (Combination == other.Combination && Cards[0] == other.Cards[0]);
        }
    }
}
