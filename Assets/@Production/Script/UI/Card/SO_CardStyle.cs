using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Pker.Style
{
    [CreateAssetMenu(fileName = "CardStyle_" , menuName = "Card/Style" , order = 0)]
    public class SO_CardStyle : ScriptableObject
    {
        [SerializeField]
        TMP_FontAsset fontAsset;
        public TMP_FontAsset Font => fontAsset;

        [SerializeField]
        SymbolStyle diamond;
        [SerializeField]
        SymbolStyle club;
        [SerializeField]
        SymbolStyle heart;
        [SerializeField]
        SymbolStyle spade;

        public SymbolStyle GetStyle(CardSymbol symbol)
        {
            switch (symbol)
            {
                case CardSymbol.Diamond:
                    return diamond;
                case CardSymbol.Club:
                    return club;
                case CardSymbol.Heart:
                    return heart;
                case CardSymbol.Spade:
                    return spade;
            }
            return null;
        }
    }

    [System.Serializable]
    public class SymbolStyle
    {
        [SerializeField]
        Color color;
        public Color Color => color;

        [SerializeField]
        Sprite symbol;
        public Sprite Symbol => symbol;
    }
}
