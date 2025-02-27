using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseComponent : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject MenuScene;
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Home()
    {
        Time.timeScale = 1f;

        // Lưu trạng thái để UIManager biết không mở Login Screen nữa
        PlayerPrefs.SetInt("ReturnFromGame", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }


}
