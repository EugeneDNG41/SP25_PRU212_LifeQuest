using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using static UnityEngine.ParticleSystem;
using System.Collections.Generic;

public class LoadingManager : MonoBehaviour
{
    public Text loadingText;
    public Slider progressBar;
    public GameObject loadingScene;
    public GameObject gameScene;

    private FirebaseFirestore firestore;

    private bool loadingComplete = false;

    public GameManager GameManager;
    public FirestoreManager firestoreManager;

    private async void Start()
    {
        GameManager = GameManager.Instance;
        firestoreManager = FirestoreManager.Instance;
        if (GameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        await LoadGameData(); // Load data before enabling scene activation
        loadingComplete = true;
        
        loadingText.text = "Press any key to continue...";
    }

    private void Update()
    {
        if (loadingComplete && Input.anyKeyDown)
        {           
            loadingScene.SetActive(false);
            gameScene.SetActive(true);
            GameManager.StartGame();
            loadingComplete = false;
        }
    }
    private async Task LoadGameData()
    {
        if (firestore == null)
        {
            firestore = FirebaseFirestore.DefaultInstance;
            if (firestore == null)
            {
                Debug.LogError("Firestore is null");
            }
            else
            {
                Debug.Log("Firestore is ready");
            }
        }

        // Start loading process
        loadingText.text = "Loading game data...";
        await Task.Delay(500); // Simulate initial delay

        // Track progress
        int totalSteps = 9; // Adjust based on number of collections loaded
        int currentStep = 0;
        if (firestoreManager.traits.Count == 0)
        {
            await LoadCollection("traits", firestoreManager.traits);

        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.stages.Count == 0)
        {
            await LoadCollection("stages", firestoreManager.stages);
        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.statImpacts.Count == 0)
        {
            await LoadCollection("stat_impacts", firestoreManager.statImpacts);
        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.ageRanges.Count == 0)
        {
            await LoadCollection("age_ranges", firestoreManager.ageRanges);
        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.scenarios.Count == 0)
        {
            await LoadCollection("scenarios", firestoreManager.scenarios);
        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.quizzes.Count == 0)
        {
            await LoadCollection("quizzes", firestoreManager.quizzes);
        }
        UpdateProgress(++currentStep, totalSteps);
        if (firestoreManager.deaths.Count == 0)
        {
            await LoadCollection("deaths", firestoreManager.deaths);
        }
        UpdateProgress(++currentStep, totalSteps);
        foreach (var scenario in firestoreManager.scenarios)
        {
            if (scenario.Value.Choices.Count == 0) await LoadCollection($"scenarios/{scenario.Key}/choices", scenario.Value.Choices);
            foreach (var choice in scenario.Value.Choices)
            {
                if (choice.Value.QuizId == null && choice.Value.Outcomes.Count == 0)
                {
                    await LoadCollection($"scenarios/{scenario.Key}/choices/{choice.Key}/outcomes", choice.Value.Outcomes);
                }
            }
        }
        UpdateProgress(++currentStep, totalSteps);
        foreach (var quiz in firestoreManager.quizzes)
        {
            if (quiz.Value.Outcomes.Count == 0) await LoadCollection($"quizzes/{quiz.Key}/outcomes", quiz.Value.Outcomes);
            if (quiz.Value.Questions.Count == 0) await LoadCollection($"quizzes/{quiz.Key}/questions", quiz.Value.Questions);
            foreach (var question in quiz.Value.Questions)
            {
                if (question.Value.Answers.Count == 0) await LoadCollection($"quizzes/{quiz.Key}/questions/{question.Key}/answers", question.Value.Answers);
            }
        }
        UpdateProgress(++currentStep, totalSteps);

        // Finalizing references
        loadingText.text = "Finalizing data...";
        await Task.Delay(500);

        foreach (var scenario in firestoreManager.scenarios.Values)
        {
            if (firestoreManager.ageRanges.ContainsKey(scenario.AgeRangeId))
                scenario.AgeRange = firestoreManager.ageRanges[scenario.AgeRangeId];

            if (scenario.RequiredTraitId != null && firestoreManager.traits.ContainsKey(scenario.RequiredTraitId))
                scenario.RequiredTrait = firestoreManager.traits[scenario.RequiredTraitId];

            foreach (var choice in scenario.Choices.Values)
            {
                if (choice.RequiredTraitId != null && firestoreManager.traits.ContainsKey(choice.RequiredTraitId))
                    choice.RequiredTrait = firestoreManager.traits[choice.RequiredTraitId];

                foreach (var outcome in choice.Outcomes.Values)
                {
                    if (firestoreManager.statImpacts.ContainsKey(outcome.ImpactId))
                        outcome.Impact = firestoreManager.statImpacts[outcome.ImpactId];

                    if (outcome.ResultTraitId != null && firestoreManager.traits.ContainsKey(outcome.ResultTraitId))
                        outcome.ResultTrait = firestoreManager.traits[outcome.ResultTraitId];
                }
            }
        }

        foreach (var quiz in firestoreManager.quizzes.Values)
        {
            foreach (var outcome in quiz.Outcomes.Values)
            {
                if (firestoreManager.statImpacts.ContainsKey(outcome.ImpactId))
                    outcome.Impact = firestoreManager.statImpacts[outcome.ImpactId];
                if (outcome.ResultTraitId != null && firestoreManager.traits.ContainsKey(outcome.ResultTraitId))
                    outcome.ResultTrait = firestoreManager.traits[outcome.ResultTraitId];
            }
        }

        foreach (var stage in firestoreManager.stages.Values)
        {
            if (firestoreManager.ageRanges.ContainsKey(stage.AgeRangeId))
                stage.AgeRange = firestoreManager.ageRanges[stage.AgeRangeId];
        }

        // Mark as completed
        loadingText.text = "Press any key to continue...";
        progressBar.value = 1;
    }

    private void UpdateProgress(int currentStep, int totalSteps)
    {
        float progress = (float)currentStep / totalSteps;
        loadingText.text = $"Loading... {progress * 100:F0}%";
        progressBar.value = progress;
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
}