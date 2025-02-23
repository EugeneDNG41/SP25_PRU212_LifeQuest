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

    private async void Start()
    {
        GameManager = GameManager.Instance;

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
        int totalSteps = 40; // Adjust based on number of collections loaded
        int currentStep = 0;

        await LoadCollection("traits", GameManager.traits);
        UpdateProgress(++currentStep, totalSteps);
        await LoadCollection("stages", GameManager.stages);
        UpdateProgress(++currentStep, totalSteps);
        await LoadCollection("stat_impacts", GameManager.statImpact);
        UpdateProgress(++currentStep, totalSteps);
        await LoadCollection("age_ranges", GameManager.ageRanges);
        UpdateProgress(++currentStep, totalSteps);
        await LoadCollection("scenarios", GameManager.scenarios);
        UpdateProgress(++currentStep, totalSteps);

        foreach (var scenario in GameManager.scenarios)
        {
            await LoadCollection($"scenarios/{scenario.Key}/choices", scenario.Value.Choices);
            UpdateProgress(++currentStep, totalSteps);

            foreach (var choice in scenario.Value.Choices)
            {
                await LoadCollection($"scenarios/{scenario.Key}/choices/{choice.Key}/outcomes", choice.Value.Outcomes);
                UpdateProgress(++currentStep, totalSteps);
            }
        }

        // Finalizing references
        loadingText.text = "Finalizing data...";
        await Task.Delay(500);

        foreach (var scenario in GameManager.scenarios.Values)
        {
            if (GameManager.ageRanges.ContainsKey(scenario.AgeRangeId))
                scenario.AgeRange = GameManager.ageRanges[scenario.AgeRangeId];

            if (scenario.RequiredTraitId != null && GameManager.traits.ContainsKey(scenario.RequiredTraitId))
                scenario.RequiredTrait = GameManager.traits[scenario.RequiredTraitId];

            foreach (var choice in scenario.Choices.Values)
            {
                if (choice.RequiredTraitId != null && GameManager.traits.ContainsKey(choice.RequiredTraitId))
                    choice.RequiredTrait = GameManager.traits[choice.RequiredTraitId];

                foreach (var outcome in choice.Outcomes.Values)
                {
                    if (GameManager.statImpact.ContainsKey(outcome.ImpactId))
                        outcome.Impact = GameManager.statImpact[outcome.ImpactId];

                    if (outcome.ResultTraitId != null && GameManager.traits.ContainsKey(outcome.ResultTraitId))
                        outcome.ResultTrait = GameManager.traits[outcome.ResultTraitId];
                }
            }
        }

        foreach (var stage in GameManager.stages.Values)
        {
            if (GameManager.ageRanges.ContainsKey(stage.AgeRangeId))
                stage.AgeRange = GameManager.ageRanges[stage.AgeRangeId];
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