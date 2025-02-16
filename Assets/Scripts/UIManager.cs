using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject mainMenuUI;
    public GameObject LoadSaveUI;
    public GameObject SettingUI;

    public LoadDataManager LoadDataManager;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
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
        loginUI.SetActive(false);
        mainMenuUI.SetActive(true);
        SettingUI.SetActive(false);
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
        await LoadDataManager.StartLoadingProcess();
        Debug.Log("LoadSaveUI loaded successfully!");
    }
    public void SettingMenuScreen() // Login button
    {
        SettingUI.SetActive(true);
        mainMenuUI.SetActive(false);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void Quit()
    {
        // Quit button
        Application.Quit();
    }
    public void OnApplicationQuit()
    {
        // Save data here      
    }
    private void Start()
    {
        if (LoadSaveUI == null)
        {
            Debug.LogError("LoadSaveUI is missing! Trying to find it in the scene...");
            LoadSaveUI = GameObject.Find("LoadSaveUI");

            if (LoadSaveUI == null)
            {
                Debug.LogError("LoadSaveUI is STILL missing! Check your scene hierarchy.");
            }
        }
    }
}

