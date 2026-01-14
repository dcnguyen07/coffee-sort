using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIToast : MonoBehaviour
{
    public static UIToast Instance;

    [SerializeField] private CanvasGroup uiToast;
    [SerializeField] private Text uiTxt;
    [SerializeField] private RectTransform uiBack;

    private bool isShowing = false;

    private void Awake()
    {
        uiToast.alpha = 0;
    }

    /// <summary>
    /// Show Toast with specific message
    /// </summary>
    public void ShowToast(string message, float duration = 1.5f)
    {
        if (isShowing) return; 

        isShowing = true;
        uiTxt.text = message;
        uiToast.alpha = 1;

        uiBack.anchoredPosition = new Vector2(uiBack.anchoredPosition.x, 0);
        uiBack.DOAnchorPosY(100, 0.5f * duration).SetDelay(0.5f * duration).SetEase(Ease.InQuad);
        uiToast.DOFade(0, 0.5f * duration).SetDelay(0.5f * duration).OnComplete(() =>
        {
            isShowing = false;
        });

    }

}