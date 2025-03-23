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
        if (GameManager.Instance.currentPlayer.Value.DeathId != null)
        {
            LoadDataManager.Instance.loadedPlayer = new KeyValuePair<string, Player>(null, null);
        } else
        {
            LoadDataManager.Instance.loadedPlayer = GameManager.Instance.currentPlayer;
        }

        // Unload the gameplay scene and return to main menu
        SceneManager.UnloadSceneAsync("GameScene");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.MainMenuScreen();
        }

    }


}
