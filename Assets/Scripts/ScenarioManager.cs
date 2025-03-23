using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }
    private FirestoreManager firestoreManager;
    private GameManager gameManager;
    private PlayedScenario currentPlayedScenario;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);
        firestoreManager = FirestoreManager.Instance;
        gameManager = GameManager.Instance;
    }

    public void NextTurn()
    {
        if (gameManager.currentPlayer.Value.Age >= 100)
        {
            Debug.Log("Game Over - Player reached maximum age.");
            return;
        }

        Scenario currentScenario = SelectScenario();
        if (currentScenario != null)
        {
            gameManager.DisplayScenario(currentScenario);
        }
        else
        {
            Debug.LogWarning("No scenario found for this age.");
        }
    }
    private Scenario SelectScenario()
    {
        List<KeyValuePair<string, Scenario>> validScenarios = firestoreManager.scenarios
        .Where(s => s.Value.AgeRange != null
                    && gameManager.currentPlayer.Value.Age >= s.Value.AgeRange.MinAge
                    && gameManager.currentPlayer.Value.Age <= s.Value.AgeRange.MaxAge
                    && !gameManager.currentPlayer.Value.PlayedScenarios.ContainsKey(s.Key)
                    && (s.Value.RequiredTraitId == null || gameManager.currentPlayer.Value.UnlockedTraits.ContainsKey(s.Value.RequiredTraitId)))
        .ToList();

        if (validScenarios.Count == 0)
        {
            gameManager.currentPlayer.Value.PlayedScenarios.Clear();
            validScenarios = firestoreManager.scenarios
                .Where(s => s.Value.AgeRange != null
                            && gameManager.currentPlayer.Value.Age >= s.Value.AgeRange.MinAge
                            && gameManager.currentPlayer.Value.Age <= s.Value.AgeRange.MaxAge)
                .ToList();
        }

        if (validScenarios.Count > 0)
        {
            var selectedEntry = validScenarios[Random.Range(0, validScenarios.Count)];
            gameManager.currentPlayer.Value.ScenarioId = selectedEntry.Key;
            currentPlayedScenario = new PlayedScenario
            {
                ScenarioDecription = selectedEntry.Value.Description
            };
            
            return selectedEntry.Value;
        }

        return null;
        
    }

    public void SelectChoice(Choice choice)
    {
        currentPlayedScenario.ChoiceDescription = choice.Description;
        var scenarioId = gameManager.currentPlayer.Value.ScenarioId;
        gameManager.currentPlayer.Value.PlayedScenarios.Add(scenarioId, currentPlayedScenario);
        if (choice.QuizId != null)
        {
            QuizManager.Instance.StartQuiz(choice.QuizId);
        }
        else
        {
            gameManager.lastSelectedButton = gameManager.choiceButtonMapping.FirstOrDefault(x => x.Value == choice).Key;           
            Outcome selectedOutcome = choice.Outcomes.Values.ToList()[Random.Range(0, choice.Outcomes.Count)];
   
            //gameManager.currentPlayer.Value.PlayedScenarios[scenarioId].OutcomeDescription = selectedOutcome.Description;   
            GameManager.Instance.ApplyOutcome(selectedOutcome);
        }
    }
}
