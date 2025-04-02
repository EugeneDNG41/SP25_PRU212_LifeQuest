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
    private ScenarioManager scenarioManager;
    private QuizManager quizManager;
    public FlipPanel flipDeath;
    public FlipPanel flipReasonDeath;
    public KeyValuePair<string, Player> currentPlayer;
    public Dictionary<Button, object> choiceButtonMapping = new();
    public Button lastSelectedButton;

    [SerializeField] public Button ChoiceA;
    [SerializeField] public Button ChoiceB;
    [SerializeField] public Button ChoiceC;
    [SerializeField] public Button ChoiceD;

    [SerializeField] public Text ChoiceTextA;
    [SerializeField] public Text ChoiceTextB;
    [SerializeField] public Text ChoiceTextC;
    [SerializeField] public Text ChoiceTextD;
    [Header("Player Elements")]
    [SerializeField] private GameObject DeathPanel;
    [SerializeField] private GameObject ReasonDeath;
    [SerializeField] private TMP_Text reasonText;

    private GameObject Toddler;
    private GameObject Child;
    private GameObject Adolescent;
    private GameObject MiddleAge;
    private GameObject Elder;
    private GameObject Death;
    private GameObject Adult;

    [Header("Quiz Elements")]
    //[SerializeField] public Text TimerText;

    string reasonDeath = "";
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);
        firestoreManager = FirestoreManager.Instance;

        
    }

    public void StartGame()
    {
        GameObject ageManager = GameObject.Find("Canvas/GameScene/AgeManager");
        Toddler = ageManager.transform.Find("Toddler").gameObject;
        Child = ageManager.transform.Find("Child").gameObject;
        Adolescent = ageManager.transform.Find("Adolescent").gameObject;
        Adult = ageManager.transform.Find("Adult").gameObject;
        MiddleAge = ageManager.transform.Find("MiddleAge").gameObject;
        Elder = ageManager.transform.Find("Elder").gameObject;
        Death = ageManager.transform.Find("Death").gameObject;
        if (LoadDataManager.Instance.loadedPlayer.Value != null)
        {
            currentPlayer = LoadDataManager.Instance.loadedPlayer;
            Scenario scenario = firestoreManager.scenarios[currentPlayer.Value.ScenarioId];
            DisplayScenario(scenario);
        } else
        {
            currentPlayer = new KeyValuePair<string, Player>(System.Guid.NewGuid().ToString(), new Player());
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
        var scenarioText = GameObject.Find("Scenario").GetComponent<Text>();
        scenarioText.text = scenario.Description;
        DisplayChoices(shuffledChoices);
    }

    public void DisplayQuestion(QuizQuestion question)
    {
        var shuffledAnswers = question.Answers.Values
                            .OrderBy(c => Random.Range(0, int.MaxValue)) // Shuffle choices
                            .ToList();
        var scenarioText = GameObject.Find("Scenario").GetComponent<Text>();
        scenarioText.text = question.Description;
        DisplayChoices(shuffledAnswers);
    }
    private void DisplayChoices<T>(List<T> choices)
    {
        Button[] choiceButtons = new Button[]
    {
        GameObject.Find("A").GetComponent<Button>(),
        GameObject.Find("B").GetComponent<Button>(),
        GameObject.Find("C").GetComponent<Button>(),
        GameObject.Find("D").GetComponent<Button>()
    };

        Text[] choiceTexts = new Text[]
        {
        GameObject.Find("AText").GetComponent<Text>(),
        GameObject.Find("BText").GetComponent<Text>(),
        GameObject.Find("CText").GetComponent<Text>(),
        GameObject.Find("DText").GetComponent<Text>()
        };
        //Button[] choiceButtons = { ChoiceA, ChoiceB, ChoiceC, ChoiceD };
        //Text[] choiceTexts = { ChoiceTextA, ChoiceTextB, ChoiceTextC, ChoiceTextD };

        choiceButtonMapping.Clear();
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
        currentPlayer.Value.Age++;
        if (outcome.ResultTraitId != null)
        {
            currentPlayer.Value.UnlockedTraits.Add(outcome.ResultTraitId, outcome.ResultTrait);
        }

        foreach (var stage in firestoreManager.stages)
        {
            if (currentPlayer.Value.Age >= stage.Value.AgeRange.MinAge && currentPlayer.Value.Age <= stage.Value.AgeRange.MaxAge)
            {
                currentPlayer.Value.StageId = stage.Key;
                break;
            }
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
        
        UpdateUI();
        ScenarioManager.Instance.NextTurn();
    }
    private void UpdateUI()
    {
        var playerName = GameObject.Find("Name").GetComponent<Text>();
        var playerAge = GameObject.Find("Age").GetComponent<Text>();
        var lifeStage = GameObject.Find("Stage").GetComponent<Text>();
        playerName.text = currentPlayer.Value.Name;
        playerAge.text = currentPlayer.Value.Age.ToString();

        GetAnimationTriggerByAge(currentPlayer.Value.Age);
        PlayDeathImage(currentPlayer.Value);
        CheckIfDead(currentPlayer.Value);

        foreach (var stage in firestoreManager.stages)
        {
            if (currentPlayer.Value.Age >= stage.Value.AgeRange.MinAge && currentPlayer.Value.Age <= stage.Value.AgeRange.MaxAge)
            {
                lifeStage.text = stage.Value.Name;
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

    private void CheckIfDead(Player player)
    {
        var death = new KeyValuePair<string, Death>();
        if (player.Age >= 100)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "AGE100").FirstOrDefault();          
        }
        if (player.Health <= 0)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "HEALTH0" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (player.Happiness <= 0)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "HAPPINESS0" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (player.Wealth <= 0)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "WEALTH0" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (player.Health >= 100)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "HEALTH100" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (player.Happiness >= 100)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "HAPPINESS100" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (player.Wealth >= 100)
        {
            death = firestoreManager.deaths.Where(d => d.Value.Cause == "WEALTH100" && d.Value.StageId == player.StageId).FirstOrDefault();
        }
        if (death.Value != null)
        {
            Debug.Log("Game Over - Player has died.");
            player.DeathId = death.Key;
            DeathPanel.SetActive(true);
            reasonDeath = death.Value.Description;
            Task.FromResult(firestoreManager.SaveToFirestore($"users/{AuthManager.Instance.User.UserId}/players", currentPlayer.Key, currentPlayer.Value));
            return;
        }
        return;
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
