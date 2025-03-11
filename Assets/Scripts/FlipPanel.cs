using System.Collections;
using UnityEngine;

public class FlipPanel : MonoBehaviour
{
    public float flipDuration = 0.5f; 
    private bool hasFlipped = false;

    private void OnEnable()
    {
        if (!hasFlipped) 
        {
            StartCoroutine(FlipAnimation());
        }
    }

    private IEnumerator FlipAnimation()
    {
        hasFlipped = true;
        RectTransform rectTransform = GetComponent<RectTransform>();
        float time = 0f;
        Vector3 originalScale = rectTransform.localScale;

        Debug.Log("FlipPanel started flipping");

        while (time < flipDuration)
        {
            float scaleX = Mathf.Lerp(1f, 0f, time / flipDuration); 
            rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = new Vector3(0f, 1f, 1f); 
        yield return new WaitForSeconds(0.1f);

        while (time < flipDuration * 2)
        {
            float scaleX = Mathf.Lerp(0f, 1f, (time - flipDuration) / flipDuration); 
            rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = originalScale;
        Debug.Log("FlipPanel finished flipping");
    }
}