using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFill : MonoBehaviour
{
    public int maxValue;
    public Image Heartfill;
    public Image Happinessfill;
    public Image Wealthfill;

    private int currenHealthValue;
    private int currenHappinessValue;
    private int currenWealthValue;
    // Reference to the GameManager
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager Instance is NULL! Make sure it's initialized before accessing UIFill.");
            return;
        }

        if (gameManager.currentPlayer.Value == null)
        {
            Debug.LogError("loadedPlayer is NULL! Ensure it's assigned in GameManager before UIFill starts.");
            return;
        }

        UpdateUI();
    }


    // Update is called once per frame
    void UpdateUI()
    {
        if (gameManager == null || gameManager.currentPlayer.Value == null) return;

        currenHealthValue = gameManager.currentPlayer.Value.Health;
        currenHappinessValue = gameManager.currentPlayer.Value.Happiness;
        currenWealthValue = gameManager.currentPlayer.Value.Wealth;

        if (Heartfill != null)
            Heartfill.fillAmount = (float)currenHealthValue / maxValue;
        else
            Debug.LogError("Heartfill Image is not assigned in the Inspector!");

        if (Happinessfill != null)
            Happinessfill.fillAmount = (float)currenHappinessValue / maxValue;
        else
            Debug.LogError("Happinessfill Image is not assigned in the Inspector!");

        if (Wealthfill != null)
            Wealthfill.fillAmount = (float)currenWealthValue / maxValue;
        else
            Debug.LogError("Wealthfill Image is not assigned in the Inspector!");
    }

    void Update()
    {
        UpdateUI();
    }

}
