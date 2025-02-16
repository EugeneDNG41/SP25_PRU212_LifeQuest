
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoadSceneUIManager : MonoBehaviour
{
    public GameObject loadingPanel; // Assign in Inspector
    public Slider loadingSlider; // Assign in Inspector
    public Text loadingText; // Assign in Inspector

    public async Task ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        float duration = 7f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            loadingSlider.value = progress;
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            await Task.Yield();
        }

        loadingPanel.SetActive(false);
    }
}