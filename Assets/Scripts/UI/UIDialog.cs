using UnityEngine;
using Managers;
using Components;

public class UIDialog : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show dialog with scale-in and fade-in effect
    /// </summary>
    public virtual void Show()
    {
        ShowWithEffect();
    }

    /// <summary>
    /// Hide dialog with scale-out and fade-out effect
    /// </summary>
    public virtual void Hide()
    {
        if (GameManager.instance.currentState == GameState.InPause)
            GameManager.instance.currentState = GameState.Playing;
        HideWithEffect();
    }

    /// <summary>
    /// Show dialog with effect
    /// </summary>
    public void ShowWithEffect()
    {
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Hide dialog with effect
    /// </summary>
    public void HideWithEffect()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() => gameObject.SetActive(false));
    }
}