using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UILoading : MonoBehaviour
{
    [Header("Time (in seconds) before hiding the loading screen")]
    public float loadingDuration = 2f;

    [Header("CanvasGroup for fade-out effect")]
    public CanvasGroup canvasGroup;

    private void Start()
    {
        // Ensure the loading screen is fully visible at start
        canvasGroup.alpha = 1f;

        // Start the delay countdown and fade-out process
        StartCoroutine(HideLoadingAfterDelay());
    }

    /// <summary>
    /// Waits for a given duration, then fades out and hides the loading screen.
    /// </summary>
    IEnumerator HideLoadingAfterDelay()
    {
        // Wait before hiding
        yield return new WaitForSeconds(loadingDuration);

        // Fade out canvas group over 0.5 seconds, then disable the GameObject
        canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}