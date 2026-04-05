//CardVisualFeedback.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardVisualFeedback : MonoBehaviour
{
    private Image cardImage;

    void Awake()
    {
        cardImage = GetComponent<Image>();
    }

    public void Flash(Color color, float duration = 0.2f)
    {
        StartCoroutine(FlashEffect(color, duration));
    }

    IEnumerator FlashEffect(Color color, float duration)
    {
        Color originalColor = cardImage.color;
        cardImage.color = color;
        yield return new WaitForSeconds(duration);
        cardImage.color = originalColor;
    }

    public void FadeOutAndDestroy()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        for (float t = 1; t >= 0; t -= Time.deltaTime * 2)
        {
            cg.alpha = t;
            yield return null;
        }

        Destroy(gameObject);
    }
}
