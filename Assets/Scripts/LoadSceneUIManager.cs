
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoadSceneUIManager : MonoBehaviour
{
    public static LoadSceneUIManager Instance { get; private set; }
    public GameObject loadingPanel; // Assign in Inspector
    public Slider loadingSlider; // Assign in Inspector
    public Text loadingText; // Assign in Inspector

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public async Task ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        float duration = 5f;
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