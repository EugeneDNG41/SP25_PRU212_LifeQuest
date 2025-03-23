using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }
    private FirestoreManager firestoreManager;
    private GameManager gameManager;

    private Quiz currentQuiz;
    private List<QuizQuestion> questionList;
    private int correctAnswers = 0;
    private int questionIndex = 0;
    private float timer;
    private bool timerActive;
    private Coroutine timerCoroutine;
    [SerializeField] public Text timerText;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);
        firestoreManager = FirestoreManager.Instance;
        gameManager = GameManager.Instance;
        if (firestoreManager == null)
        {
            Debug.LogError("FirestoreManager not found!");
            return;
        }
    }

    private void Update()
    {
        if (timerActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timerActive = false;
                SelectRandomAnswer();
            }
        }
    }

    public void StartQuiz(string quizId)
    {
        if (!firestoreManager.quizzes.TryGetValue(quizId, out currentQuiz))
        {
            Debug.LogError($"Quiz ID {quizId} not found.");
            return;
        }

        correctAnswers = 0;
        questionIndex = 0;
        questionList = currentQuiz.Questions.Values.ToList();
        timerText.gameObject.SetActive(true);
        DisplayNextQuestion();
    }

    private void DisplayNextQuestion()
    {
        if (questionIndex < currentQuiz.Questions.Count)
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            gameManager.DisplayQuestion(questionList[questionIndex]);
            timerCoroutine = StartCoroutine(StartQuestionTimer());
        }
        else
        {
            SubmitQuiz();
        }
    }
    private IEnumerator StartQuestionTimer()
    {
        float timeRemaining = 10f; // Adjust the time limit as needed

        while (timeRemaining > 0)
        {
            timerText.text = $"{Mathf.Ceil(timeRemaining)}";
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        timerText.text = "Time's up!";
        SelectRandomAnswer();
    }
    public void SelectAnswer(QuizAnswer answer)
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (answer.IsCorrect) correctAnswers++;
        questionIndex++;
        gameManager.lastSelectedButton = gameManager.choiceButtonMapping.FirstOrDefault(x => x.Value == answer).Key;
        DisplayNextQuestion();
    }
    private void SelectRandomAnswer()
    {
        var availableAnswers = gameManager.choiceButtonMapping.Values.OfType<QuizAnswer>().ToList();
        if (availableAnswers.Count > 0)
        {
            QuizAnswer randomAnswer = availableAnswers[Random.Range(0, availableAnswers.Count)];
            SelectAnswer(randomAnswer);
        }
    }
    private void SubmitQuiz()
    {
        float correctPercentage = (float)correctAnswers / currentQuiz.Questions.Count;
        string outcomeKey = correctPercentage >= 0.6f ? "Success" : "Failure";
        Outcome quizOutcome = currentQuiz.Outcomes[outcomeKey];
        timerText.gameObject.SetActive(false);
        gameManager.ApplyOutcome(quizOutcome);
    }
}
