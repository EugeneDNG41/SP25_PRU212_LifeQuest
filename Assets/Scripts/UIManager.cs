using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject mainMenuUI;
    public GameObject LoadSaveUI;
    public GameObject SettingUI;
    [SerializeField] public Button ContinueButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void RegisterScreen() // Register button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }
    public void MainMenuScreen() // Login button
    {
        if (LoadDataManager.Instance.loadedPlayer.Key != null)
        {
            ContinueButton.interactable = true;
        }
        else
        {
            ContinueButton.interactable = false;
        }
        loginUI.SetActive(false);
        SettingUI.SetActive(false);
        LoadSaveUI.SetActive(false);
        mainMenuUI.SetActive(true);
        
    }
    public async void LoadSaveScreen()
    {
        if (LoadSaveUI == null)
        {
            Debug.LogError("LoadSaveUI is null! Make sure it is assigned in the Inspector.");
            return; // Prevent further errors
        }

        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);
        
        LoadSaveUI.SetActive(true);
      
        await LoadDataManager.Instance.LoadGameData();
        Debug.Log("LoadSaveUI loaded successfully!");
    }
    public void SettingMenuScreen()
    {
        SettingUI.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    public void NewGame()
    {
        LoadDataManager.Instance.loadedPlayer = new();
        StartGame();
    }
    public void StartGame()
    {
        mainMenuUI.SetActive(false);

        // Load GameScene additively
        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
    }
    public void Quit()
    {
        // Quit button
        Application.Quit();
    }

}

