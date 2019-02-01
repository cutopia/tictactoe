using System.Collections;
using UnityEngine;

public class MenuGroupFader : MonoBehaviour
{
    public float fadeDuration = 1.0f;
    public bool isVisible = false;
    public bool fadeInOnStart = true;
    private Coroutine fadeRoutine = null;

    /**
     * if the item is set to visible and is set to fade in on start
     * then we fade it in to give an opening effect.    
     */
    void Start()
    {
        if (isVisible && fadeInOnStart)
        {
            gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 0;
            Show();
        }
    }

    /**
     * Shows the UI item by making it active and fading it in.
     */
    public void Show()
    {
        isVisible = true;
        if (!gameObject.activeSelf)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 0;
            gameObject.SetActive(true);
        }
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
        fadeRoutine = StartCoroutine(Fade(gameObject, fadeDuration, true));
    }

    /**
     * Starts the process of fading out and hiding the UI item
     */
    public void Hide()
    {
        isVisible = false;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
        fadeRoutine = StartCoroutine(Fade(gameObject, fadeDuration, false));
    }

    /**
     * coroutine that changes alpha over time heading from the current alpha to
     * the target.
     * @param gameObject to animate the alpha of
     * @param duration (float) speed of the effect
     * @param fadingIn (bool) denotes if we are moving the alpha towards 1.0 (true) or 0 (false)
     */
    protected IEnumerator Fade(GameObject gameObject, float duration, bool fadingIn)
    {
        float startingAlpha = 0;
        float startTime = Time.time;
        float progress = 0;
        var grp = gameObject.GetComponent<CanvasGroup>();

        if (fadingIn)
        {
            startingAlpha = grp.alpha;

            while (grp.alpha < 1.0)
            {
                yield return new WaitForEndOfFrame();
                progress = Time.time - startTime;
                grp.alpha = Mathf.Clamp01(startingAlpha + (progress / duration));
            }
        }
        else
        {
            startingAlpha = grp.alpha;

            while (grp.alpha > float.Epsilon)
            {
                yield return new WaitForEndOfFrame();
                progress = Time.time - startTime;
                grp.alpha = Mathf.Clamp01(startingAlpha - (progress / duration));
            }
            gameObject.SetActive(false);
        }
    }
}