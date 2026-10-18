using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ScreenFader : Singleton<ScreenFader>
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private bool startBlack = true;
    public IEnumerator FadeOut() => FadeTo(1f);
    public IEnumerator FadeIn() => FadeTo(0f);

    private void OnEnable()
    {
        SetAlpha(startBlack ? 1f : 0f);
    }

    IEnumerator FadeTo(float alpha)
    {
        float startAlpha = canvasGroup.alpha;
        if(fadeDuration > 0f)
        {
            float elapsed = 0f;
            while(elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, elapsed / fadeDuration);
                yield return null;
            }
            SetAlpha(alpha);
        }   
    }
    private void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;

        canvasGroup.blocksRaycasts = alpha > 0f;
    }
}
