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
    private Player pendingPlayer;
    public Transform traitsGrid;
    public GameObject traitsWindow;
    public Button traitsButton;
    public Button scenariosButton;
    private bool isTraitsExpanded = false;
    private bool isScenariosExpanded = false;
    private Player confirmedPlayer;

    private LoadSceneUIManager loadSceneUIManager;
    private FirestoreManager firestoreManager;
    private Dictionary<string, string> traitsDictionary = new Dictionary<string, string>();
    public KeyValuePair<string, Player> currentPlayer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        loadSceneUIManager = LoadSceneUIManager.Instance;
        firestoreManager = FirestoreManager.Instance;
        StartCoroutine(WaitForFirestoreManager());
        LoadTraits();
    }

    IEnumerator WaitForFirestoreManager()
    {
        while (FirestoreManager.Instance == null)
        {
            yield return null;
        }
        firestoreManager = FirestoreManager.Instance;
        loadSceneUIManager = LoadSceneUIManager.Instance;
    }
    void Start()
    {
        traitsButton.onClick.AddListener(ToggleTraits);

        // Mặc định ẩn 2 grid
        traitsWindow.gameObject.SetActive(false);
    }
    public async Task StartLoadingProcess()
    {
        await LoadGameData();
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
        }

        foreach (Transform child in gridParent) Destroy(child.gameObject);

        List<Task<GameObject>> tileTasks = new List<Task<GameObject>>();
        int totalPlayers = firestoreManager.players.Count;
        int count = 0;

        foreach (var player in firestoreManager.players.Values)
        {
            Task<GameObject> tileTask = CreatePlayerTile(player);
            tileTasks.Add(tileTask);
            Debug.Log($"📥 Loading player: {player.Name}, Age: {player.Age}");
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

    private async Task<GameObject> CreatePlayerTile(Player data)
    {
        GameObject tile = Instantiate(playerTilePrefab, gridParent);
        tile.SetActive(true);

        tile.transform.Find("NameBG/Name").GetComponent<Text>().text = data.Name;
        tile.transform.Find("HealthBG/Age").GetComponent<Text>().text = $"Age: {Mathf.Max(0, data.Age - 1)}";
        tile.transform.Find("NameBG/Sex").GetComponent<Text>().text = $"{data.Sex}";
        tile.transform.Find("StatusBG/Happiness").GetComponent<Text>().text = $"{data.Happiness}";
        tile.transform.Find("StatusBG/Health").GetComponent<Text>().text = $"{data.Health}";
        tile.transform.Find("StatusBG/Wealth").GetComponent<Text>().text = $"{data.Wealth}";
        tile.transform.Find("HealthBG/Status").GetComponent<Text>().text = $"{data.Status}";

        Button button = tile.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPlayerTileClicked(data));
        }

        await Task.Yield();
        return tile;
    }

    private void OnPlayerTileClicked(Player selectedPlayer)
    {
        if (selectedPlayer == null) return;

        pendingPlayer = selectedPlayer;
        confirmPlayerName.text = pendingPlayer.Name;
        confirmPlayerAge.text = $"Age: {pendingPlayer.Age}";
        confirmPlayerSex.text = $"Sex: {pendingPlayer.Sex}";
        confirmPlayerHappiness.text = $"{pendingPlayer.Happiness}";
        confirmPlayerHealth.text = $"{pendingPlayer.Health}";
        confirmPlayerWealth.text = $"{pendingPlayer.Wealth}";

        foreach (Transform child in traitsGrid) Destroy(child.gameObject);
        foreach (var traitId in pendingPlayer.UnlockedTraits.Keys)
        {
            string traitName = traitsDictionary.ContainsKey(traitId) ? traitsDictionary[traitId] : "Unknown Trait";
            GameObject traitItem = new GameObject("TraitItem", typeof(Text));
            traitItem.transform.SetParent(traitsGrid);

            Text textComponent = traitItem.GetComponent<Text>();
            textComponent.text = traitName;
            textComponent.font = confirmPlayerName.font;
            textComponent.fontSize = 100;
        }

        int traitsCount = pendingPlayer.UnlockedTraits.Count;
        traitsButton.GetComponentInChildren<Text>().text = $"Unlocked Traits ({traitsCount})";

        Button playButton = confirmLoadWindow.transform.Find("Buttons/PlayButton")?.GetComponent<Button>();
        Button quitButton = confirmLoadWindow.transform.Find("Buttons/Quit")?.GetComponent<Button>();
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => PlayGame(pendingPlayer));
        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(() => QuitLoad());

        SavePanel.SetActive(false);
        confirmLoadWindow.SetActive(true);
    }
    public void ExitGrid()
    {
        traitsWindow.gameObject.SetActive(false);
    }
    private void ToggleTraits()
    {

        Debug.Log($"✅ ToggleTraits");
        isTraitsExpanded = !isTraitsExpanded;
        traitsWindow.gameObject.SetActive(isTraitsExpanded);
    }

    
    private async void LoadTraits()
    {
        CollectionReference traitsCollection = FirebaseFirestore.DefaultInstance.Collection("traits");
        QuerySnapshot snapshot = await traitsCollection.GetSnapshotAsync();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                string traitId = document.Id;
                string traitName = document.GetValue<string>("Name");
                traitsDictionary[traitId] = traitName;
                Debug.Log($"Loaded Trait: {traitId} - {traitName}"); // Debug log added
            }
        }
    }
    private void PlayGame(Player player)
    {
        if (player == null)
        {
            Debug.LogError("❌ Cannot start game: player is NULL!");
            return;
        }

        if (firestoreManager == null || firestoreManager.players == null)
        {
            Debug.LogError("❌ FirestoreManager or players dictionary is NULL!");
            return;
        }

        string playerKey = firestoreManager.players.FirstOrDefault(p => p.Value == player).Key;
        if (string.IsNullOrEmpty(playerKey))
        {
            Debug.LogError("❌ Player key not found in FirestoreManager!");
            return;
        }

        currentPlayer = new KeyValuePair<string, Player>(playerKey, player);
        Debug.Log($"🎮 Starting game with player: {currentPlayer.Value.Name}");

        SceneManager.LoadScene("GameScene");
    }
    private void QuitLoad()
    {
        confirmLoadWindow.gameObject.SetActive(false);
        SavePanel.gameObject.SetActive(true);
    }
}