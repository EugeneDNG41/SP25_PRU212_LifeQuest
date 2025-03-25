using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class FirestoreManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static FirestoreManager Instance;
    private FirebaseFirestore firestore;

    public Dictionary<string, Scenario> scenarios = new();
    public Dictionary<string, Stage> stages = new();
    public Dictionary<string, Trait> traits = new();
    public Dictionary<string, StatImpact> statImpact = new();
    public Dictionary<string, AgeRange> ageRanges = new();
    public Dictionary<string, Death> deaths = new();
    public Dictionary<string, Quiz> quizzes = new();
    public Dictionary<string, Achievement> achievements = new();
    public Dictionary<string, Player> players = new();

    private void Awake()
    {
        InitializeFirestore();
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void InitializeFirestore()
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
    public async Task<string> ToCSV()
    {
        var sb = new StringBuilder();
        foreach (var property in typeof(Trait).GetProperties())
        {
            sb.Append(property.Name).Append(',');
        }
        await LoadCollection("traits", traits);
        foreach (var trait in traits)
        {
            sb.Append('\n').Append(trait.Key.ToString()).Append(',').Append(trait.Value.ToString());
        }
        return sb.ToString();
    }
    public void SaveToFile()
    {
        // Use the CSV generation from before
        var content = Task.FromResult(ToCSV());

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, "export.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
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
        await LoadCollection("quizzes", quizzes);
        foreach (var quiz in quizzes)
        {
            await LoadCollection($"quizzes/{quiz.Key}/questions", quiz.Value.Questions);
            await LoadCollection($"quizzes/{quiz.Key}/outcomes", quiz.Value.Outcomes);
            Debug.Log($"Loaded {quiz.Value.Questions.Count} questions for quiz {quiz.Value.Description}");
            Debug.Log($"Loaded {quiz.Value.Outcomes.Count} outcomes for quiz {quiz.Value.Description}");
            foreach (var question in quiz.Value.Questions)
            {
                await LoadCollection($"quizzes/{quiz.Key}/questions/{question.Key}/choices", question.Value.Answers);
                Debug.Log($"Loaded {question.Value.Answers.Count} choices for question {question.Value.Description}");
            }
        }
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
    public async Task LoadCollection<T>(string collectionName, Dictionary<string, T> storage) where T : class, new()
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
    public async Task SaveToFirestore<T>(string collectionName, string documentId, T data)
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
}
