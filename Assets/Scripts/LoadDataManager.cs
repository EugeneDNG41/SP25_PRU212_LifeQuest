using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadDataManager : MonoBehaviour
{
    public static LoadDataManager Instance { get; private set; }
    public GameObject playerTilePrefab;
    public Transform gridParent;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image loadingBG;
    public GameObject confirmLoadWindow;
    public GameObject SavePanel;
    public Text confirmPlayerName;
    public Text confirmPlayerAge;
    public Text confirmPlayerSex;
    public Text confirmPlayerHappiness;
    public Text confirmPlayerHealth;
    public Text confirmPlayerWealth;
    public Text confirmPlayerStatus;
    private KeyValuePair<string, Player> pendingPlayer;

    private FirestoreManager firestoreManager;
    public KeyValuePair<string, Player> loadedPlayer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        //else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        firestoreManager = FirestoreManager.Instance;
    }

    public async Task LoadGameData()
    {
        if (AuthManager.Instance.User == null) return;
        if (firestoreManager == null) return;
        await LoadPlayers(AuthManager.Instance.User.UserId);
    }

    public async Task LoadPlayers(string uid)
    {
        loadingSlider.gameObject.SetActive(true);
        loadingSlider.value = 0;
        loadingText.text = "Loading... 0%";

        confirmLoadWindow.SetActive(false);
        if (firestoreManager.players.Count == 0)
        {
            await firestoreManager.LoadCollection($"users/{uid}/players", firestoreManager.players);
            firestoreManager.players = firestoreManager.players.Where(currentPlayer => currentPlayer.Value != null
                                                && currentPlayer.Value.Health > 0
                                                && currentPlayer.Value.Health < 100
                                                && currentPlayer.Value.Happiness > 0
                                                && currentPlayer.Value.Happiness < 100
                                                && currentPlayer.Value.Wealth > 0
                                                && currentPlayer.Value.Wealth < 100
                                                && string.IsNullOrEmpty(currentPlayer.Value.DeathId)
                                                && !string.IsNullOrEmpty(currentPlayer.Value.ScenarioId)
                                                && currentPlayer.Value.Age < 100).ToDictionary(p => p.Key, p => p.Value);
        }

        foreach (Transform child in gridParent) Destroy(child.gameObject);

        List<Task<GameObject>> tileTasks = new List<Task<GameObject>>();
        int totalPlayers = firestoreManager.players.Count;
        int count = 0;

        foreach (var player in firestoreManager.players)
        {
            Task<GameObject> tileTask = CreatePlayerTile(player);
            tileTasks.Add(tileTask);
            Debug.Log($"📥 Loading player: {player.Value.Name}, Age: {player.Value.Age}");
            count++;
            float progress = (float)count / totalPlayers;
            loadingSlider.value = progress;
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            await tileTask;
        }

        await Task.WhenAll(tileTasks);

        loadingSlider.value = 1;
        loadingText.text = "Load Complete! Click to continue!";
        await WaitForUserClick();

        loadingSlider.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
        loadingBG.gameObject.SetActive(false);
        SavePanel.SetActive(true);
    }

    private async Task WaitForUserClick()
    {
        while (!Input.GetMouseButtonDown(0))
        {
            await Task.Yield();
        }
    }

    private async Task<GameObject> CreatePlayerTile(KeyValuePair<string, Player> data)
    {
        GameObject tile = Instantiate(playerTilePrefab, gridParent);
        tile.SetActive(true);

        tile.transform.Find("NameBG/Name").GetComponent<Text>().text = data.Value.Name;
        tile.transform.Find("HealthBG/Age").GetComponent<Text>().text = $"Age: {Mathf.Max(0, data.Value.Age - 1)}";
        tile.transform.Find("NameBG/Sex").GetComponent<Text>().text = $"{data.Value.Sex}";
        tile.transform.Find("StatusBG/Happiness").GetComponent<Text>().text = $"{data.Value.Happiness}";
        tile.transform.Find("StatusBG/Health").GetComponent<Text>().text = $"{data.Value.Health}";
        tile.transform.Find("StatusBG/Wealth").GetComponent<Text>().text = $"{data.Value.Wealth}";
        tile.transform.Find("HealthBG/Status").GetComponent<Text>().text = $"{data.Value.Status}";

        Button button = tile.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPlayerTileClicked(data));
        }

        await Task.Yield();
        return tile;
    }

    private void OnPlayerTileClicked(KeyValuePair<string, Player> selectedPlayer)
    {
        if (selectedPlayer.Value == null) return;

        pendingPlayer = selectedPlayer;
        confirmPlayerName.text = pendingPlayer.Value.Name;
        confirmPlayerAge.text = $"Age: {pendingPlayer.Value.Age}";
        confirmPlayerSex.text = $"Sex: {pendingPlayer.Value.Sex}";
        confirmPlayerHappiness.text = $"{pendingPlayer.Value.Happiness}";
        confirmPlayerHealth.text = $"{pendingPlayer.Value.Health}";
        confirmPlayerWealth.text = $"{pendingPlayer.Value.Wealth}";

        Button playButton = confirmLoadWindow.transform.Find("Buttons/PlayButton")?.GetComponent<Button>();
        Button quitButton = confirmLoadWindow.transform.Find("Buttons/Quit")?.GetComponent<Button>();
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => PlayGame(pendingPlayer));
        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(() => QuitLoad());

        SavePanel.SetActive(false);
        confirmLoadWindow.SetActive(true);
    }

    private void PlayGame(KeyValuePair<string, Player> player)
    {
        if (player.Value == null)
        {
            Debug.LogError("❌ Cannot start game: player is NULL!");
            return;
        }
        loadedPlayer = pendingPlayer;
        Debug.Log($"🎮 Starting game with player: {loadedPlayer.Value.Name}");

        SceneManager.LoadScene("GameScene");
    }
    private void QuitLoad()
    {
        confirmLoadWindow.gameObject.SetActive(false);
        SavePanel.gameObject.SetActive(true);
    }
}