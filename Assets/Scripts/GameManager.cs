using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private FirestoreManager firestoreManager;
    public FlipPanel flipDeath;
    public FlipPanel flipReasonDeath;
    public KeyValuePair<string, Player> currentPlayer;
    //public Player loadedPlayer;
    public Dictionary<Button, object> choiceButtonMapping = new();
    public Button lastSelectedButton;


    [Header("Player Elements")]
    [SerializeField] private Text PlayerName;
    [SerializeField] private Text PlayerAge;
    [SerializeField] private Text LifeStage;
    [SerializeField] private GameObject DeathPanel;
    [SerializeField] private GameObject ReasonDeath;
    [SerializeField] private TMP_Text reasonText;

    [Header("Scenario Elements")]
    [SerializeField] private Text ScenarioText;
    [SerializeField] private Text ChoiceTextA;
    [SerializeField] private Text ChoiceTextB;
    [SerializeField] private Text ChoiceTextC;
    [SerializeField] private Text ChoiceTextD;
    [SerializeField] private Button ChoiceA;
    [SerializeField] private Button ChoiceB;
    [SerializeField] private Button ChoiceC;
    [SerializeField] private Button ChoiceD;

    [Header("Player Image")]
    [SerializeField] public GameObject Toddler;
    [SerializeField] public GameObject Child;
    [SerializeField] public GameObject Adolescent;
    [SerializeField] public GameObject MiddleAge;
    [SerializeField] public GameObject Elder;
    [SerializeField] public GameObject Death;
    [SerializeField] public GameObject Adult;

    [Header("Quiz Elements")]
    [SerializeField] public Text TimerText;

    string reasonDeath = "";
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        firestoreManager = FirestoreManager.Instance;
    }

    public void StartGame()
    {
        if (LoadDataManager.Instance.loadedPlayer.Value != null)
        {
            currentPlayer = LoadDataManager.Instance.loadedPlayer;
            Scenario scenario = firestoreManager.scenarios[currentPlayer.Value.ScenarioId];
            DisplayScenario(scenario);
        } else
        {
            currentPlayer = new KeyValuePair<string, Player>(GUID.Generate().ToString(), new Player());
            currentPlayer.Value.Sex = Random.Range(0, 1) > 0.5 ? "Male" : "Female";
            currentPlayer.Value.Name = GenerateRandomName();
            UpdateUI();
            ScenarioManager.Instance.NextTurn();
        }
        
        UpdateUI();
        ScenarioManager.Instance.NextTurn();
    }
    private string GenerateRandomName()
    {
        string[] firstNames = { "John", "Emma", "Michael", "Olivia", "William" };
        string[] lastNames = { "Smith", "Johnson", "Brown", "Davis", "Miller" };
        return $"{firstNames[Random.Range(0, firstNames.Length)]} {lastNames[Random.Range(0, lastNames.Length)]}";
    }

    public void DisplayScenario(Scenario scenario)
    {
        var shuffledChoices = scenario.Choices.Values
                            .Where(c => c.RequiredTraitId == null || c.RequiredTraitId.Split(',')
                             .Any(id => currentPlayer.Value.UnlockedTraits.Keys.Contains(id.Trim())))
                            .OrderBy(c => Random.Range(0, int.MaxValue)) // Shuffle choices
                            .ToList();
        ScenarioText.text = scenario.Description;
        DisplayChoices(shuffledChoices);
    }

    public void DisplayQuestion(QuizQuestion question)
    {
        var shuffledAnswers = question.Answers.Values
                            .OrderBy(c => Random.Range(0, int.MaxValue)) // Shuffle choices
                            .ToList();
        ScenarioText.text = question.Description;
        DisplayChoices(shuffledAnswers);
    }
    private void DisplayChoices<T>(List<T> choices)
    {
        Button[] choiceButtons = { ChoiceA, ChoiceB, ChoiceC, ChoiceD };
        Text[] choiceTexts = { ChoiceTextA, ChoiceTextB, ChoiceTextC, ChoiceTextD };

        for (int i = 0; i < choices.Count; i++)
        {
            choiceButtonMapping[choiceButtons[i]] = choices[i];
            choiceButtons[i].onClick.RemoveAllListeners();
            if (choices[i] is QuizAnswer answer)
            {
                choiceTexts[i].text = answer.Description;

                choiceButtons[i].onClick.AddListener(() => QuizManager.Instance.SelectAnswer(answer));
            }
            else if (choices[i] is Choice choice)
            {
                choiceTexts[i].text = choice.Description;
                choiceButtons[i].onClick.AddListener(() => ScenarioManager.Instance.SelectChoice(choice));
            }
        }
        for (int i = choices.Count; i < 4; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }
    }

    public void ApplyOutcome(Outcome outcome)
    {
        // Disable all choice buttons
        foreach (var button in choiceButtonMapping.Keys)
        {
            button.interactable = false;
        }

        if (lastSelectedButton != null)
        {
            Text buttonText = lastSelectedButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = outcome.Description;
            }
        }
        Debug.Log($"Outcome: {outcome.Description}");
        currentPlayer.Value.PlayedScenarios[currentPlayer.Value.ScenarioId].OutcomeDescription = outcome.Description;
        currentPlayer.Value.Health += outcome.Impact.HealthImpact;
        currentPlayer.Value.Happiness += outcome.Impact.HappinessImpact;
        currentPlayer.Value.Wealth += outcome.Impact.WealthImpact;

        if (outcome.ResultTraitId != null)
        {
            currentPlayer.Value.UnlockedTraits.Add(outcome.ResultTraitId, outcome.ResultTrait);
        }

        StartCoroutine(WaitForNextClick());
    }
    private IEnumerator WaitForNextClick()
    {
        if (AuthManager.Instance.User != null)
        {
            Task.FromResult(firestoreManager.SaveToFirestore($"users/{AuthManager.Instance.User.UserId}/players", currentPlayer.Key, currentPlayer.Value));
        }
        yield return new WaitForSeconds(0.5f); 
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0)); 
        yield return new WaitForSeconds(0.5f); 


        ProceedToNextScenario();
    }
    private void ProceedToNextScenario()
    {
        foreach (var button in choiceButtonMapping.Keys)
        {
            button.interactable = true;
        }
        currentPlayer.Value.Age++;
        UpdateUI();
        ScenarioManager.Instance.NextTurn();
    }
    private void UpdateUI()
    {
        PlayerName.text = currentPlayer.Value.Name;
        PlayerAge.text = currentPlayer.Value.Age.ToString();

        GetAnimationTriggerByAge(currentPlayer.Value.Age);
        PlayDeathImage(currentPlayer.Value);
        GameOver(currentPlayer.Value);

        foreach (var stage in firestoreManager.stages)
        {
            if (currentPlayer.Value.Age >= stage.Value.AgeRange.MinAge && currentPlayer.Value.Age <= stage.Value.AgeRange.MaxAge)
            {
                LifeStage.text = stage.Value.Name;
                currentPlayer.Value.StageId = stage.Key;
                break;
            }
        }
    }

    private void PlayDeathImage(Player player)
    {      
        if (player.Health <= 0 || player.Happiness <= 0 || player.Wealth <= 0 ||
            player.Health >= 100 || player.Happiness >= 100 || player.Wealth >= 100
            || player.Age >= 100)
        {
            SetDeath();
        }      
        
        else Death.SetActive(false);

    }

    private void GameOver(Player currentPlayer)
    {
        if (currentPlayer.Age >= 100)
        {
            Debug.Log("Game Over - Player reached maximum age.");
            DeathPanel.SetActive(true);
            reasonDeath = "You lived a century, but slipped on a banana peel during your birthday party.";
            return;
        }
        if (currentPlayer.Health <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You exercised so hard that your muscles decided to quit... permanently.";
            return;
        }
        if (currentPlayer.Happiness <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You were truly unfortunate, passing away forever with no one by your side.";
            return;
        }
        if (currentPlayer.Wealth <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You sold your soul to pay off your debts. Turns out, the soul market crashed";
            return;
        }
        if (currentPlayer.Health >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You became so fit that you ran straight into another dimension. Nobody’s seen you since.";
            return;
        }
        if (currentPlayer.Happiness >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You laughed so hard at a joke that you simply evaporated into pure joy.";
            return;
        }
        if (currentPlayer.Wealth >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            DeathPanel.SetActive(true);
            reasonDeath = "You bought the entire planet, but now there’s nothing left to buy. You died of boredom.";
            return;
        }
    }
    public void DeathButton()
    {   
        Debug.Log("DeathButton clicked!");
        DeathPanel.SetActive(false);
        ReasonDeath.SetActive(true);
        reasonText.text = reasonDeath;
    }


    private void SetDeath()
    {
        Death.SetActive(true);
        Toddler.SetActive(false);
        Child.SetActive(false);
        Adult.SetActive(false);
        Adolescent.SetActive(false);
        Elder.SetActive(false);
        MiddleAge.SetActive(false);
    }
    private void GetAnimationTriggerByAge(int age)
    {
        Debug.Log("GetAnimationTriggerByAge loaded successfully!");
        if (age >= 0 && age <= 4) Toddler.SetActive(true); // toddler
        if (age >= 5 && age <= 10)// Child
        {
            Toddler.SetActive(false);
            Child.SetActive(true);
        }
        if (age >= 11 && age <= 17)// Adolescent 
        {
            Adolescent.SetActive(true);
            Child.SetActive(false);
        }
        if (age >= 18 && age <= 39) //Adult 
        {
            Adolescent.SetActive(false);
            Adult.SetActive(true);

        }
        if (age >= 40 && age <= 64) //Middle Age
        {
            Adult.SetActive(false);
            MiddleAge.SetActive(true);
        }
    
        if (age >= 65 && age <= 100) //Elder 
        {
            MiddleAge.SetActive(false);
            Elder.SetActive(true);
        }
    }
    
}
