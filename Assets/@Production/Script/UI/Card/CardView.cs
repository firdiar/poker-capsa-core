using DG.Tweening;
using Pker;
using Pker.Style;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pker.UI
{
    public class CardView : MonoBehaviour
    {
        [SerializeField]
        SO_CardStyle style;

        [SerializeField]
        TextMeshProUGUI[] cardText;

        [SerializeField]
        Image[] cardSymbols;

        [SerializeField]
        Image background;
        [SerializeField]
        Image shadowBg;


        public void Initialize(Card card)
        {
            var cStyle = style.GetStyle(card.Symbol);
            foreach (var cardSymbol in cardSymbols) 
            {
                cardSymbol.sprite = cStyle.Symbol;
            }

            foreach (var text in cardText)
            {
                text.font = style.Font;
                text.SetText(card.Number.AsString());
            }
        }

        public void SetAlpha(float alpha, float duration) 
        {
            foreach (var cardSymbol in cardSymbols)
            {
                cardSymbol.DOFade(alpha, duration);
            }

            foreach (var text in cardText)
            {
                text.DOFade(alpha, duration);
            }

            background.DOFade(alpha, duration);
            shadowBg.DOFade(alpha, duration);
        }
    }
}
