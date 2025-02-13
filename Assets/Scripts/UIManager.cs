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
}

