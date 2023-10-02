using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pker
{
    //card can be converted to byte and vice-versa
    [System.Serializable]
    public struct Card : IEquatable<Card>
    {
        public CardNumber Number;
        public CardSymbol Symbol;

        public Card(CardSymbol symbol, CardNumber number)
        { 
            Number = number;
            Symbol = symbol;
        }

        public Card(byte encodedByte)
        {
            // Extracting Number and Symbol from the byte
            Number = GetNumber(encodedByte);
            Symbol= GetSymbol(encodedByte);
        }

        public byte ToByte()
        {
            // Using 4 bits each for Number and Symbol to fit them in one byte
            return (byte)(((byte)Number << 4) | (byte)Symbol);
        }

        public static byte GetNumberAsByte(byte b)
        {
            return (byte)(b >> 4);//shift to right
        }

        public static CardNumber GetNumber(byte b)
        {
            return (CardNumber)(b >> 4);//shift to right
        }

        public static CardSymbol GetSymbol(byte b)
        {
            return (CardSymbol)(b & 0x0F);//remove 4 bit ahead
        }

        public static Card GetCard(byte b)
        {
            return new Card(b);
        }

        public bool Equals(Card other)
        {
            return Number == other.Number && Symbol == other.Symbol;
        }
    }
}