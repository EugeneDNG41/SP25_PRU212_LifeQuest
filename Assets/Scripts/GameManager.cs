using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Firestore firestore;
    public FlipPanel flipDeath;
    public FlipPanel flipReasonDeath;

    public Player currentPlayer;
    private Scenario currentScenario;
    private Dictionary<Button, Choice> choiceButtonMapping = new();
    private HashSet<string> playedScenarios = new();

    [Header("Player Elements")]
    [SerializeField] private TMP_Text PlayerName;
    [SerializeField] private TMP_Text PlayerAge;
    [SerializeField] private TMP_Text LifeStage;
    [SerializeField] private TMP_Text PlayerHealth;
    [SerializeField] private TMP_Text PlayerHappiness;
    [SerializeField] private TMP_Text PlayerWealth;
    [SerializeField] private GameObject Death;
    [SerializeField] private GameObject ReasonDeath;
    [SerializeField] private TMP_Text reasonText;

    [Header("Scenario Elements")]
    [SerializeField] private TMP_Text ScenarioText;
    [SerializeField] private TMP_Text ChoiceTextA;
    [SerializeField] private TMP_Text ChoiceTextB;
    [SerializeField] private TMP_Text ChoiceTextC;
    [SerializeField] private TMP_Text ChoiceTextD;
    [SerializeField] private Button ChoiceA;
    [SerializeField] private Button ChoiceB;
    [SerializeField] private Button ChoiceC;
    [SerializeField] private Button ChoiceD;
    

    public Dictionary<string, Scenario> scenarios = new();
    public Dictionary<string, Stage> stages = new();
    public Dictionary<string, Trait> traits = new();
    public Dictionary<string, StatImpact> statImpact = new();
    public Dictionary<string, AgeRange> ageRanges = new();

    string reasonDeath = "";
    void Awake()
    {     
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
    //private async void Start()
    //{
    //    if (firestore == null)
    //    {
    //        InitializeFirestore();
    //    }
    //    await LoadGameData();

    //    Debug.Log("All game data loaded. Ready to start!");
    //}
    private void InitializeFirestore()
    {
        firestore = Firestore.DefaultInstance;
        if (firestore == null)
        {
            Debug.LogError("Firestore is null");
        }
        else
        {
            Debug.Log("Firestore is ready");
        }
    }
    public async Task LoadGameData()
    {
        if (firestore == null)
        {
            InitializeFirestore();
        }
        await LoadCollection("traits", traits);
        await LoadCollection("stages", stages);
        await LoadCollection("stat_impacts", statImpact);
        await LoadCollection("age_ranges", ageRanges);
        await LoadCollection("scenarios", scenarios);

        foreach (var scenario in scenarios)
        {
            await LoadCollection($"scenarios/{scenario.Key}/choices", scenario.Value.Choices);
            Debug.Log($"Loaded {scenario.Value.Choices.Count} choices for scenario {scenario.Value.Description}");

            foreach (var choice in scenario.Value.Choices)
            {
                await LoadCollection($"scenarios/{scenario.Key}/choices/{choice.Key}/outcomes", choice.Value.Outcomes);
                Debug.Log($"Loaded {choice.Value.Outcomes.Count} outcomes for choice {choice.Value.Description}");
            }
        }
        foreach (var scenario in scenarios.Values)
        {
            if (ageRanges.ContainsKey(scenario.AgeRangeId))
            {
                scenario.AgeRange = ageRanges[scenario.AgeRangeId];
                Debug.Log($"Loaded scenario {scenario.Description} with age range {scenario.AgeRange.MinAge}-{scenario.AgeRange.MaxAge}");
            }
            if (scenario.RequiredTrait != null && traits.ContainsKey(scenario.RequiredTraitId))
            {
                scenario.RequiredTrait = traits[scenario.RequiredTraitId];
                Debug.Log($"Loaded scenario {scenario.Description} with required trait {scenario.RequiredTrait.Name}");
            }
            foreach (var choice in scenario.Choices.Values)
            {
                if (choice.RequiredTraitId != null && traits.ContainsKey(choice.RequiredTraitId))
                {
                    choice.RequiredTrait = traits[choice.RequiredTraitId];
                    Debug.Log($"Loaded choice {choice.Description} with required trait {choice.RequiredTrait.Name}");
                }
                foreach (var outcome in choice.Outcomes.Values)
                {
                    if (statImpact.ContainsKey(outcome.ImpactId))
                    {
                        outcome.Impact = statImpact[outcome.ImpactId];
                        Debug.Log($"Loaded outcome {outcome.Description} with impact {outcome.Impact.HappinessImpact}/{outcome.Impact.HealthImpact}/{outcome.Impact.WealthImpact}");
                    }
                    if (outcome.ResultTraitId != null && traits.ContainsKey(outcome.ResultTraitId))
                    {
                        outcome.ResultTrait = traits[outcome.ResultTraitId];
                        Debug.Log($"Loaded outcome {outcome.Description} with result trait {outcome.ResultTrait.Name}");
                    }
                }
            }
        }
        foreach (var stage in stages.Values)
        {
            if (ageRanges.ContainsKey(stage.AgeRangeId))
            {
                stage.AgeRange = ageRanges[stage.AgeRangeId];
                Debug.Log($"Loaded stage {stage.Name} with age range {stage.AgeRange.MinAge}-{stage.AgeRange.MaxAge}");
            }
        }
    }
    private async Task LoadCollection<T>(string collectionName, Dictionary<string, T> storage) where T : class, new()
    {
        var snapshot = await firestore.Collection(collectionName).GetSnapshotAsync();
        if (snapshot == null)
        {
            Debug.LogError($"Failed to load {collectionName}");
            return;
        }
        else if (snapshot.Count == 0)
        {
            Debug.LogWarning($"No items found in {collectionName}");
            return;
        }
        else
        {
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                T data = document.ConvertTo<T>();
                storage.Add(document.Id, data);
            }
            Debug.Log($"Loaded {storage.Count} items from {collectionName}");
        }
    }
    private async void AddTraits()
    {
        List<Trait> traits = new List<Trait>
        {
            new Trait { Name = "Natural Leader", Description = "People follow you—sometimes willingly, sometimes out of fear." },
            new Trait { Name = "Materialist", Description = "You believe happiness comes in two forms: cash and credit." },
            new Trait { Name = "Hard Worker", Description = "You grind so hard even coffee is worried about you." },
            new Trait { Name = "Free Spirit", Description = "Rules are just suggestions, and deadlines are just a myth." },
            new Trait { Name = "Virtuous Advocate", Description = "You fight for justice, even if it means debating a toddler over candy." },
            new Trait { Name = "Smooth Talker", Description = "You could sell sand in the desert and make a profit." },
            new Trait { Name = "Opportunist", Description = "If there's a loophole, you’ll find it—and probably monetize it." },
            new Trait { Name = "Survivor", Description = "You’ve seen things. You’ve done things. And you still refuse to pay full price." },
            new Trait { Name = "Curious Learner", Description = "You ask ‘why’ so much that Google is considering hiring you." },
            new Trait { Name = "Social Butterfly", Description = "You talk so much even Siri needs a break." },
            new Trait { Name = "Old Soul", Description = "You reminisce about ‘the good old days’…even if you’re 12." },
            new Trait { Name = "Mastermind", Description = "You have a plan for everything—except what to eat for dinner." }
        };

        for (int i = 5; i < traits.Count + 5; i++)
        {
            string docId = $"TR{i}"; // Unique document ID
            await SaveToFirestore("traits", docId, traits[i]);
        }
    }
    private async void AddAgeRanges()
    {
        List<AgeRange> ageRanges = new List<AgeRange>
        {
            new AgeRange { MinAge = 0, MaxAge = 4 },
            new AgeRange { MinAge = 5, MaxAge = 10 },
            new AgeRange { MinAge = 11, MaxAge = 17 },
            new AgeRange { MinAge = 18, MaxAge = 39 },
            new AgeRange { MinAge = 40, MaxAge = 64 },
            new AgeRange { MinAge = 65, MaxAge = 100 },
            new AgeRange { MinAge = 0, MaxAge = 0 },
            
            new AgeRange { MinAge = 1, MaxAge = 3 },
            new AgeRange { MinAge = 4, MaxAge = 4 },
            new AgeRange { MinAge = 5, MaxAge = 9 },
            new AgeRange { MinAge = 10, MaxAge = 10 },
            new AgeRange { MinAge = 18, MaxAge = 18 },      
            new AgeRange { MinAge = 19, MaxAge = 39 }
            
        };

        for (int i = 1; i < ageRanges.Count + 1 ; i++)
        {
            string docId = $"AGE{i}"; // Unique document ID
            await SaveToFirestore("age_ranges", docId, ageRanges[i]);
        }
    }
    private async void AddStages()
    {
        List<Stage> stages = new List<Stage>
        {
            new Stage { Name = "Toddler", AgeRangeId = "AGE1", Description = "A time of pure chaos, drool, and unexpected naps." },
            new Stage { Name = "Child", AgeRangeId = "AGE2", Description = "The golden years of sugar highs and questionable decision-making." },
            new Stage { Name = "Adolescent", AgeRangeId = "AGE3", Description = "Where bad haircuts and bad decisions go hand in hand." },
            new Stage { Name = "Young Adult", AgeRangeId = "AGE4", Description = "You have freedom, responsibilities, and absolutely no idea what you're doing." },
            new Stage { Name = "Middle Age", AgeRangeId = "AGE5", Description = "You finally have wisdom... and back pain to go with it." },
            new Stage { Name = "Elder", AgeRangeId = "AGE6", Description = "You’ve seen it all, done it all, and now just want a nap." }
        };

        for (int i = 1; i < stages.Count + 1; i++)
        {
            string docId = $"ST{i}"; // Firestore-friendly ID
            await SaveToFirestore("stages", docId, stages[i]);
        }
    }
    private async void AddImpact()
    {
        List<StatImpact> impacts = new List<StatImpact>
        {
            new StatImpact {HappinessImpact = 0, HealthImpact = 0, WealthImpact = 0},
            new StatImpact {HappinessImpact = 10, HealthImpact = 10, WealthImpact = 10},

            new StatImpact {HappinessImpact = 0, HealthImpact = 10, WealthImpact = 10},
            new StatImpact {HappinessImpact = 10, HealthImpact = 0, WealthImpact = 10},
            new StatImpact {HappinessImpact = 10, HealthImpact = 10, WealthImpact = 0},

            new StatImpact {HappinessImpact = 10, HealthImpact = 0, WealthImpact = 0},
            new StatImpact {HappinessImpact = 0, HealthImpact = 10, WealthImpact = 0},
            new StatImpact {HappinessImpact = 0, HealthImpact = 0, WealthImpact = 10},

            new StatImpact {HappinessImpact = -10, HealthImpact = -10, WealthImpact = -10},

            new StatImpact {HappinessImpact = 0, HealthImpact = -10, WealthImpact = -10},
            new StatImpact {HappinessImpact = -10, HealthImpact = 0, WealthImpact = -10},
            new StatImpact {HappinessImpact = -10, HealthImpact = -10, WealthImpact = 0},

            new StatImpact {HappinessImpact = 0, HealthImpact = 0, WealthImpact = -10},
            new StatImpact {HappinessImpact = -10, HealthImpact = 0, WealthImpact = 0},
            new StatImpact {HappinessImpact = 0, HealthImpact = -10, WealthImpact = 0},

        };

        for (int i = 1; i < impacts.Count + 1; i++)
        {
            string docId = $"I{i}"; // Unique document ID
            await SaveToFirestore("stat_impacts", docId, impacts[i]);
        }
    }
    private async void AddScenarioWithChoicesAndOutcomes(
        string scenarioId,
        string requiredTraitId,
        string ageRangeId,
        string description,
        List<(string choiceDescription, string choiceRequiredTraitId, List<(string outcomeDescription, string impactId, string resultTraitId)>)> choicesWithOutcomes)
    {
        DocumentReference scenarioRef = firestore.Collection("scenarios").Document(scenarioId);

        // Create Scenario Object
        Scenario scenario = new Scenario
        {
            RequiredTraitId = requiredTraitId,
            AgeRangeId = ageRangeId,
            Description = description,
            Choices = new Dictionary<string, Choice>()
        };

        // Add Scenario to Firestore
        await scenarioRef.SetAsync(scenario);
        Debug.Log($"Scenario '{description}' added successfully!");
        int i = 1;
        int j = 1;
        // Add Choices & Outcomes
        foreach (var (choiceDescription, choiceRequiredTraitId, outcomes) in choicesWithOutcomes)
        {
            DocumentReference choiceRef = scenarioRef.Collection("choices").Document($"{scenarioId}C{i}");
            i++;
            Choice choice = new Choice
            {
                RequiredTraitId = choiceRequiredTraitId,
                Description = choiceDescription
            };

            await choiceRef.SetAsync(choice);
            Debug.Log($"Choice '{choiceDescription}' added!");

            foreach (var (outcomeDescription, impactId, resultTraitId) in outcomes)
            {
                DocumentReference outcomeRef = choiceRef.Collection("outcomes").Document($"{scenarioId}C{i}O{j}");
                j++;
                Outcome outcome = new Outcome
                {
                    Description = outcomeDescription,
                    ImpactId = impactId,
                    ResultTraitId = resultTraitId
                };

                await outcomeRef.SetAsync(outcome);
                Debug.Log($"Outcome '{outcomeDescription}' added!");
            }
        }
    }
    private async Task SaveToFirestore<T>(string collectionName, string documentId, T data)
    {
        try
        {
            DocumentReference docRef = firestore.Collection(collectionName).Document(documentId);
            await docRef.SetAsync(data);
            Debug.Log($"Successfully saved {documentId} to {collectionName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving document: {e.Message}");
        }
    }
    public void StartGame()
    {
        currentPlayer = new Player();
        string[] firstNameArray = { "John", "Emma", "Michael", "Olivia", "William" };
        string[] lastNameArray = { "Smith", "Johnson", "Brown", "Davis", "Miller" };
        string firstName = firstNameArray[Random.Range(0, firstNameArray.Length)];
        string lastName = lastNameArray[Random.Range(0, lastNameArray.Length)];
        currentPlayer.Name = firstName + " " + lastName;
        playedScenarios.Clear();
        UpdateUI();
        NextTurn();
    }
    private void NextTurn()
    {
        if (currentPlayer.Age >= 100)
        {
            Debug.Log("Game Over - Player reached maximum age.");
            Death.SetActive(true);
            reasonDeath = "You lived a century, but slipped on a banana peel during your birthday party.";
            return;
        }
        if (currentPlayer.Health <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You exercised so hard that your muscles decided to quit... permanently.";
            return;
        }
        if (currentPlayer.Happiness <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You were truly unfortunate, passing away forever with no one by your side.";
            return;
        }
        if (currentPlayer.Wealth <= 0)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You sold your soul to pay off your debts. Turns out, the soul market crashed";
            return;
        }
        if (currentPlayer.Health >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You became so fit that you ran straight into another dimension. Nobody’s seen you since.";
            return;
        }
        if (currentPlayer.Happiness >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You laughed so hard at a joke that you simply evaporated into pure joy.";
            return;
        }
        if (currentPlayer.Wealth >= 100)
        {
            Debug.Log("Game Over - Player has died.");
            Death.SetActive(true);
            reasonDeath = "You bought the entire planet, but now there’s nothing left to buy. You died of boredom.";
            return;
        }
        currentScenario = SelectScenario();
        if (currentScenario != null)
        {
            DisplayScenario();
        }
        else
        {
            Debug.LogWarning("No scenario found for this age.");
        }
    }

    public void DeathButton()
    {
        Debug.Log("DeathButton clicked!"); // Kiểm tra xem có chạy không
        Death.SetActive(false);
        ReasonDeath.SetActive(true);
        reasonText.text = reasonDeath;
    }

    private Scenario SelectScenario()
    {
        List<KeyValuePair<string, Scenario>> validScenarios = scenarios
            .Where(s => s.Value.AgeRange != null
                        && currentPlayer.Age >= s.Value.AgeRange.MinAge
                        && currentPlayer.Age <= s.Value.AgeRange.MaxAge
                        && !playedScenarios.Contains(s.Key)
                        && (s.Value.RequiredTraitId == null || currentPlayer.UnlockedTraits.ContainsKey(s.Value.RequiredTraitId)))
            .ToList();

        if (validScenarios.Count == 0)
        {
            playedScenarios.Clear();
            validScenarios = scenarios
                .Where(s => s.Value.AgeRange != null
                            && currentPlayer.Age >= s.Value.AgeRange.MinAge
                            && currentPlayer.Age <= s.Value.AgeRange.MaxAge)
                .ToList();
        }

        if (validScenarios.Count > 0)
        {
            var selectedEntry = validScenarios[Random.Range(0, validScenarios.Count)];
            playedScenarios.Add(selectedEntry.Key); 
            return selectedEntry.Value; 
        }

        return null; 
    }

    private void DisplayScenario()
    {
        ScenarioText.text = currentScenario.Description;

        // Shuffle the choices before displaying them
        var shuffledChoices = currentScenario.Choices.Values
                            .Where(c => c.RequiredTraitId == null || currentPlayer.UnlockedTraits.ContainsKey(c.RequiredTraitId))
                            .OrderBy(c => Random.Range(0, int.MaxValue)) // Shuffle choices
                            .ToList();

        Button[] choiceButtons = { ChoiceA, ChoiceB, ChoiceC, ChoiceD };
        TMP_Text[] choiceTexts = { ChoiceTextA, ChoiceTextB, ChoiceTextC, ChoiceTextD };

        for (int i = 0; i < 4; i++)
        {
            choiceTexts[i].text = shuffledChoices[i].Description;
            choiceButtonMapping[choiceButtons[i]] = shuffledChoices[i];

            // Remove old listeners to prevent stacking
            choiceButtons[i].onClick.RemoveAllListeners();

            // Capture the correct choice in the lambda expression
            Choice choice = shuffledChoices[i];
            choiceButtons[i].onClick.AddListener(() => SelectChoice(choice));
        }
    }

    public void SelectChoice(Choice choice)
    {
        // Pick a random outcome from available ones
        List<Outcome> possibleOutcomes = new List<Outcome>(choice.Outcomes.Values);
        Outcome selectedOutcome = possibleOutcomes[Random.Range(0, possibleOutcomes.Count)];

        // Apply the outcome's impact
        ApplyOutcome(selectedOutcome, choice);
    }

    private void ApplyOutcome(Outcome outcome, Choice selectedChoice)
    {
        Debug.Log($"Outcome: {outcome.Description}");

        currentPlayer.Health += outcome.Impact.HealthImpact;
        currentPlayer.Happiness += outcome.Impact.HappinessImpact;
        currentPlayer.Wealth += outcome.Impact.WealthImpact;

        if (outcome.ResultTrait != null)
        {
            currentPlayer.UnlockedTraits.Add(outcome.ResultTraitId, outcome.ResultTrait);
        }
        // Disable all choice buttons
        foreach (var button in choiceButtonMapping.Keys)
        {
            button.interactable = false;
        }

        // Find the button corresponding to the selected choice
        Button selectedButton = choiceButtonMapping.FirstOrDefault(x => x.Value == selectedChoice).Key;
        if (selectedButton != null)
        {
            TMP_Text buttonText = selectedButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = outcome.Description;  // Replace button text with outcome
            }
        }
        // Listen for the next click to proceed
        StartCoroutine(WaitForNextClick());
    }
    private IEnumerator WaitForNextClick()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0)); // Wait for a click

        ProceedToNextScenario();
    }
    private void ProceedToNextScenario()
    {
        foreach (var button in choiceButtonMapping.Keys)
        {
            button.interactable = true;
        }
        currentPlayer.Age++;
        UpdateUI();
        NextTurn();
    }

    private void UpdateUI()
    {
        PlayerName.text = currentPlayer.Name;
        PlayerAge.text = currentPlayer.Age.ToString();
        PlayerHealth.text = currentPlayer.Health.ToString();
        PlayerHappiness.text = currentPlayer.Happiness.ToString();
        PlayerWealth.text = currentPlayer.Wealth.ToString();
        foreach (var stage in stages)
        {
            if (currentPlayer.Age >= stage.Value.AgeRange.MinAge && currentPlayer.Age <= stage.Value.AgeRange.MaxAge)
            {
                LifeStage.text = stage.Value.Name;
                currentPlayer.StageId = stage.Key; // Use the dictionary key as StageId
                break;
            }
        }
    }
}
