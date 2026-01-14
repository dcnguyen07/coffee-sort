using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [Header("UI Panel Settings")]
    public CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Show panel (enable UI)
    /// </summary>
    public virtual void Show()
    {
        //gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// Hide panel (disable UI)
    /// </summary>
    public virtual void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        //gameObject.SetActive(false);
    }
}