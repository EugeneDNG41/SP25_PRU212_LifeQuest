using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static FirestoreManager Instance;
    private FirebaseFirestore firestore;

    public Dictionary<string, Scenario> scenarios = new();
    public Dictionary<string, Stage> stages = new();
    public Dictionary<string, Trait> traits = new();
    public Dictionary<string, StatImpact> statImpacts = new();
    public Dictionary<string, AgeRange> ageRanges = new();
    public Dictionary<string, Death> deaths = new();
    public Dictionary<string, Quiz> quizzes = new();
    public Dictionary<string, Achievement> achievements = new();
    public Dictionary<string, Player> players = new();
    public Dictionary<string, User> users = new();

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
    public async Task<string> TraitsToCSV()
    {
        var sb = new StringBuilder();
        sb.Append("Key").Append(',');
        foreach (var property in typeof(Trait).GetProperties())
        {
            sb.Append(property.Name).Append(',');
        }
        await LoadCollection("traits", traits);
        foreach (var trait in traits)
        {
            sb.Append('\n')
          .Append($"\"{trait.Key}\"").Append(',')
          .Append($"\"{trait.Value.Name}\"").Append(',')
          .Append($"\"{trait.Value.Description}\"");
            Console.WriteLine($"Wrote trait {trait.Key}");
        }
        return sb.ToString();
    }
    public async Task<string> ScenariosToCSV()
    {
        var sb = new StringBuilder();

        // CSV Header
        sb.AppendLine("ScenarioID,ScenarioDescription,RequiredTrait,AgeRange,ChoiceID,ChoiceDescription,RequiredTrait,QuizID,OutcomeID,OutcomeDescription,Impact,ResultTrait");

        await LoadCollection("scenarios", scenarios);
        foreach (var scenario in scenarios)
        {
            await LoadCollection($"scenarios/{scenario.Key}/choices", scenario.Value.Choices);

            foreach (var choice in scenario.Value.Choices)
            {
                await LoadCollection($"scenarios/{scenario.Key}/choices/{choice.Key}/outcomes", choice.Value.Outcomes);
            }
        }

        foreach (var scenario in scenarios)
        {
            string scenarioId = scenario.Key;
            string scenarioDesc = scenario.Value.Description;
            string requiredTrait = scenario.Value.RequiredTrait?.Description ?? "None";
            string ageRange = scenario.Value.AgeRangeId ?? "Unknown";

            foreach (var choice in scenario.Value.Choices)
            {
                string choiceId = choice.Key;
                string choiceDesc = choice.Value.Description;
                string choiceRequiredTrait = choice.Value.RequiredTraitId ?? "None";
                string quizId = choice.Value.QuizId ?? "None";

                foreach (var outcome in choice.Value.Outcomes)
                {
                    string outcomeId = outcome.Key;
                    string outcomeDesc = outcome.Value.Description;
                    string resultTrait = outcome.Value.ResultTraitId ?? "None";
                    string impact = outcome.Value.ImpactId ?? "None";

                    // Add a row per (Scenario → Choice → Outcome)
                    sb.AppendLine($"{scenarioId},\"{scenarioDesc}\",\"{requiredTrait}\",\"{ageRange}\",{choiceId},\"{choiceDesc}\",\"{choiceRequiredTrait}\",\"{quizId}\",{outcomeId},\"{outcomeDesc}\",\"{impact}\",\"{resultTrait}\"");
                }

                // If a answer has no outcomes, add a row for just (Scenario → Choice)
                if (choice.Value.Outcomes.Count == 0)
                {
                    sb.AppendLine($"{scenarioId},\"{scenarioDesc}\",\"{requiredTrait}\",\"{ageRange}\",{choiceId},\"{choiceDesc}\",\"{choiceRequiredTrait}\",\"{quizId}\",,,");
                }
            }

            // If a scenario has no choices, add a row for just the scenario
            if (scenario.Value.Choices.Count == 0)
            {
                sb.AppendLine($"{scenarioId},\"{scenarioDesc}\",\"{requiredTrait}\",\"{ageRange}\",,,,");
            }
        }

        return sb.ToString();
    }

    public async Task<string> QuizzesToCSV()
    {
        var sb = new StringBuilder();

        // CSV Header
        sb.AppendLine("QuizID,QuizDescription,OutcomeID,OutcomeDescription,Impact,ResultTrait,QuestionID,QuestionDescription,AnswerID,AnswerDescription,IsCorrect");

        // Load all quizzes
        await LoadCollection("quizzes", quizzes);

        foreach (var quiz in quizzes)
        {
            // Load Outcomes and Questions for each quiz
            await LoadCollection($"quizzes/{quiz.Key}/outcomes", quiz.Value.Outcomes);
            await LoadCollection($"quizzes/{quiz.Key}/questions", quiz.Value.Questions);

            foreach (var question in quiz.Value.Questions)
            {
                // Load Answers for each question
                await LoadCollection($"quizzes/{quiz.Key}/questions/{question.Key}/answers", question.Value.Answers);
            }
        }

        // Process each quiz
        foreach (var quiz in quizzes)
        {
            string quizId = quiz.Key;
            string quizDesc = (quiz.Value.Description);

            // Process outcomes first
            foreach (var outcome in quiz.Value.Outcomes)
            {
                string outcomeId = outcome.Key;
                string outcomeDesc = outcome.Value.Description;
                string resultTrait = outcome.Value.ResultTraitId ?? "None";
                string impact = outcome.Value.ImpactId ?? "None";

                sb.AppendLine($"{quizId},\"{quizDesc}\",{outcomeId},\"{outcomeDesc}\",\"{impact}\",\"{resultTrait}\",,,,");
            }

            // If quiz has no outcomes, write it anyway
            if (quiz.Value.Outcomes.Count == 0)
            {
                sb.AppendLine($"{quizId},\"{quizDesc}\",,,,,,,,");
            }

            // Process each question
            foreach (var question in quiz.Value.Questions)
            {
                string questionId = question.Key;
                string questionDesc = (question.Value.Description);

                // Process each answer
                foreach (var answer in question.Value.Answers)
                {
                    string answerId = answer.Key;
                    string answerDesc = (answer.Value.Description);
                    string isCorrect = answer.Value.IsCorrect ? "TRUE" : "FALSE";

                    sb.AppendLine($"{quizId},\"{quizDesc}\",,,,,{questionId},\"{questionDesc}\",{answerId},\"{answerDesc}\",{isCorrect}");
                }

                // If a question has no answers, add a row for just (Quiz → Question)
                if (question.Value.Answers.Count == 0)
                {
                    sb.AppendLine($"{quizId},\"{quizDesc}\",,,,,{questionId},\"{questionDesc}\",,,");
                }
            }

            // If quiz has no questions, write it anyway
            if (quiz.Value.Questions.Count == 0)
            {
                sb.AppendLine($"{quizId},\"{quizDesc}\",,,,,,,,,");
            }
        }

        return sb.ToString();
    }
    public async Task<string> AgeRangesToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("AgeRangeID,MinAge,MaxAge");

        await LoadCollection("age_ranges", ageRanges);

        foreach (var ageRange in ageRanges)
        {
            sb.AppendLine($"{ageRange.Key},{ageRange.Value.MinAge},{ageRange.Value.MaxAge}");
        }

        return sb.ToString();
    }

    public async Task<string> DeathsToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("DeathID,Title,Description,Cause,StageID");

        await LoadCollection("deaths", deaths);

        foreach (var death in deaths)
        {
            string deathId = death.Key;
            string title = (death.Value.Title);
            string description = (death.Value.Description);
            string cause = death.Value.Cause ?? "Unknown";
            string stageId = death.Value.StageId ?? "None";

            sb.AppendLine($"{deathId},\"{title}\",\"{description}\",\"{cause}\",\"{stageId}\"");
        }

        return sb.ToString();
    }

    public async Task<string> StagesToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("StageID,Name,Description,AgeRangeID");

        await LoadCollection("stages", stages);

        foreach (var stage in stages)
        {
            string stageId = stage.Key;
            string name = (stage.Value.Name);
            string description = (stage.Value.Description);
            string ageRangeId = stage.Value.AgeRangeId ?? "None";

            sb.AppendLine($"{stageId},\"{name}\",\"{description}\",\"{ageRangeId}\"");
        }

        return sb.ToString();
    }

    public async Task<string> StatImpactsToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ImpactID,HealthImpact,HappinessImpact,WealthImpact");

        await LoadCollection("stat_impacts", statImpacts);

        foreach (var impact in statImpacts)
        {
            sb.AppendLine($"{impact.Key},{impact.Value.HealthImpact},{impact.Value.HappinessImpact},{impact.Value.WealthImpact}");
        }

        return sb.ToString();
    }

    public async Task<string> UsersToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("UserID,Username,PlayerID,PlayerName,Age,Health,Happiness,Wealth,Sex,Status,DeathID,StageID,ScenarioID");

        await LoadCollection("users", users);

        foreach (var user in users)
        {
            string userId = user.Key;
            string username = (user.Value.Username);

            await LoadCollection($"users/{userId}/players", user.Value.Players);

            foreach (var player in user.Value.Players)
            {
                string playerId = player.Key;
                string playerName = (player.Value.Name);
                int age = player.Value.Age;
                int health = player.Value.Health;
                int happiness = player.Value.Happiness;
                int wealth = player.Value.Wealth;
                string sex = player.Value.Sex ?? "Unknown";
                string status = (player.Value.Status);
                string deathId = player.Value.DeathId ?? "None";
                string stageId = player.Value.StageId ?? "None";
                string scenarioId = player.Value.ScenarioId ?? "None";

                sb.AppendLine($"{userId},\"{username}\",{playerId},\"{playerName}\",{age},{health},{happiness},{wealth},\"{sex}\",\"{status}\",\"{deathId}\",\"{stageId}\",\"{scenarioId}\"");
            }
        }

        return sb.ToString();
    }
    public async Task<string> PlayedScenariosToCSV()
    {
        var sb = new StringBuilder();
        sb.AppendLine("UserID,PlayerID,ScenarioID,ScenarioDescription,ChoiceDescription,OutcomeDescription");

        await LoadCollection("users", users);

        foreach (var user in users)
        {
            string userId = user.Key;

            await LoadCollection($"users/{userId}/players", user.Value.Players);

            foreach (var player in user.Value.Players)
            {
                string playerId = player.Key;
                await LoadCollection($"users/{userId}/players/{playerId}/playedScenarios", player.Value.PlayedScenarios);

                foreach (var scenario in player.Value.PlayedScenarios)
                {
                    string scenarioId = scenario.Key;
                    string scenarioDesc = (scenario.Value.ScenarioDecription);
                    string choiceDesc = (scenario.Value.ChoiceDescription);
                    string outcomeDesc = (scenario.Value.OutcomeDescription);

                    sb.AppendLine($"{userId},{playerId},{scenarioId},\"{scenarioDesc}\",\"{choiceDesc}\",\"{outcomeDesc}\"");
                }
            }
        }

        return sb.ToString();
    }




    public async void SaveToFile()
    {
        // Use the CSV generation from before
        var content = await PlayedScenariosToCSV();

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, "played_scenarios.csv");

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
        await LoadCollection("stat_impacts", statImpacts);
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
                Debug.Log($"Loaded {choice.Value.Outcomes.Count} outcomes for answer {choice.Value.Description}");
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
                    Debug.Log($"Loaded answer {choice.Description} with required trait {choice.RequiredTrait.Name}");
                }
                foreach (var outcome in choice.Outcomes.Values)
                {
                    if (statImpacts.ContainsKey(outcome.ImpactId))
                    {
                        outcome.Impact = statImpacts[outcome.ImpactId];
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
