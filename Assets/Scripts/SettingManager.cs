using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    Resolution[] resolutions;

    void Start()
    {
        // Load saved settings
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        fullscreenToggle.isOn = Screen.fullScreen;

        // Get available resolutions
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        foreach (Resolution res in resolutions)
        {
            resolutionDropdown.options.Add(new Dropdown.OptionData(res.width + "x" + res.height));
        }
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void BackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
