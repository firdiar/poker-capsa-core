using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Pker.UI;
using Pker;
using UnityEngine.Pool;
using Gtion.Plugin.DI;
using System.Threading.Tasks;

namespace Pker.UI
{
    public class CardHolder : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        bool interactive;
        [SerializeField]
        Vector2 startPosition;
        [SerializeField]
        Vector2 destroyPosition;
        [SerializeField]
        CardView cardView;
        [SerializeField]
        RectTransform childRect;

        [ShowNativeProperty]
        public bool IsSelected { get; private set; }

        public Card Card { get; private set; }

        [GInject]
        PlayerDeckView deckView;

        private void Start()
        {
            GDi.Request(this);
        }

        public async void Init(Card card, float delay = 0)
        {
            Card = card;
            childRect.anchoredPosition = startPosition;// new Vector2(0, -400);
            

            cardView.SetAlpha(0, 0);
            cardView.Initialize(card);

            if (delay > 0)
            {
                await Task.Delay(Mathf.FloorToInt(1000 * delay));
            }
            childRect.DOAnchorPos(Vector2.zero, 1f);
            cardView.SetAlpha(1, 1);
        }

        public void Destroy(IObjectPool<CardHolder> pool)
        {
            cardView.SetAlpha(0, 0.45f);
            childRect.DOAnchorPos(destroyPosition, 0.5f).OnComplete(()=> pool.Release(this));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactive) return;

            if (IsSelected)
            {
                deckView.UnSelectCard(Card);
            }
            else
            {
                deckView.SelectCard(Card);
            }
        }

        public void SetSelected(bool isSelected) 
        {
            this.IsSelected = isSelected;
            if (isSelected)
            {
                childRect.DOAnchorPosY(50, 0.25f);
            }
            else
            {
                childRect.DOAnchorPosY(0, 0.4f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (interactive && !IsSelected)
            {
                childRect.DOAnchorPosY(20, 0.25f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (interactive && !IsSelected)
            {
                childRect.DOAnchorPosY(0, 0.25f);
            }
        }
    }
}