using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class LoadDataManager : MonoBehaviour
{
    public static LoadDataManager Instance { get; private set; }
    public GameObject playerTilePrefab; // Assign in Inspector
    public Transform gridParent; // Assign in Inspector (GridLayoutGroup)

    private LoadSceneUIManager loadSceneUIManager; // Assign in Inspector  
    private FirestoreManager firestoreManager;
    public KeyValuePair<string, Player> currentPlayer;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        loadSceneUIManager = LoadSceneUIManager.Instance;
        firestoreManager = FirestoreManager.Instance;
    }

    public async Task StartLoadingProcess()
    {
        await LoadGameData();
        //await LoadSceneUIManager.Instance.ShowLoadingScreen();       
    }
    public async Task LoadGameData()
    {
        if (AuthManager.Instance.User != null)
        {
            await LoadPlayers(AuthManager.Instance.User.UserId);
        }      
    }
    public void ChoosePlayer() //this is just a random player for now, please implement a proper player selection
    {
        if (firestoreManager.players.Count > 0)
        {
            System.Random random = new System.Random();
            currentPlayer = firestoreManager.players.ElementAt(random.Next(firestoreManager.players.Count));
            UIManager.Instance.StartGame();
        }
    }
    public async Task LoadPlayers(string uid)
    {
        if (firestoreManager.players.Count == 0)
        {
            await firestoreManager.LoadCollection($"users/{uid}/players", firestoreManager.players);
            firestoreManager.players.Where(currentPlayer => currentPlayer.Value != null
                                                && !string.IsNullOrEmpty(currentPlayer.Value.DeathId)
                                                && !string.IsNullOrEmpty(currentPlayer.Value.ScenarioId)
                                                && currentPlayer.Value.Age < 100).ToDictionary(p => p.Key, p => p.Value);
            //foreach (var player in firestoreManager.players)
            //{
            //    //await firestoreManager.LoadCollection($"users/{uid}/players/{player.Key}/unlockedTraits", player.Value.UnlockedTraits);
            //    await firestoreManager.LoadCollection($"users/{uid}/players/{player.Key}/playedScenarios", player.Value.PlayedScenarios);
            //}
        }
             
        foreach (Transform child in gridParent) Destroy(child.gameObject);
        List<Task<GameObject>> tileTasks = new List<Task<GameObject>>();      
        foreach (var player in firestoreManager.players.Values)
        {
            tileTasks.Add(CreatePlayerTile(player));
        }
        GameObject[] tiles = await Task.WhenAll(tileTasks);
        foreach (var tile in tiles) tile.SetActive(true);

    }

    private async Task<GameObject> CreatePlayerTile(Player data)
    {
        GameObject tile = Instantiate(playerTilePrefab, gridParent);
        tile.SetActive(false);

        tile.transform.Find("NameBG/Name").GetComponent<Text>().text = data.Name;
        tile.transform.Find("HealthBG/Age").GetComponent<Text>().text = $"Age: {data.Age}";
        tile.transform.Find("NameBG/Sex").GetComponent<Text>().text = $"{data.Sex}";
        tile.transform.Find("StatusBG/Happiness").GetComponent<Text>().text = $"{data.Happiness}";
        tile.transform.Find("StatusBG/Health").GetComponent<Text>().text = $"{data.Health}";
        tile.transform.Find("StatusBG/Wealth").GetComponent<Text>().text = $"{data.Wealth}";
        tile.transform.Find("HealthBG/Status").GetComponent<Text>().text = $"{data.Status}";

        await Task.Yield();
        return tile;
    }

}
