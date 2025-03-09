using UnityEngine;
using UnityEngine.UI; // Use Legacy UI

public class PlayerTileUI : MonoBehaviour
{
    public Text playerNameText; // Change from TMP_Text to Text
    public Text playerAgeText;
    public Text playerSexText;
    public Text playerHealthText;
    public Text playerHappinessText;
    public Text playerWealthText;
    public Text playerStatusText;

    public void Setup(Player player)
    {
        playerNameText.text = player.Name;
        playerAgeText.text = $"Age: {player.Age}";
        playerSexText.text = player.Sex;
        playerHealthText.text = $"Health: {player.Health}";
        playerHappinessText.text = $"Happiness: {player.Happiness}";
        playerWealthText.text = $"Wealth: {player.Wealth}";
        playerStatusText.text = player.Status;
    }
}
