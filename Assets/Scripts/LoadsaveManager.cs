using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadDataManager : MonoBehaviour
{
    public GameObject playerTilePrefab; // Assign in Inspector
    public Transform gridParent; // Assign in Inspector (GridLayoutGroup)

    public LoadSceneUIManager loadSceneUIManager; // Assign in Inspector
    public static LoadDataManager Instance { get; private set; }
    private FirebaseFirestore firestore;
    public static UIManager instance;
    public Player currentPlayer;
    private List<Player> playerList = new();
    public Dictionary<string, Player> Players = new();

    void Awake()
    {   
        firestore = FirebaseFirestore.DefaultInstance;
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

    public async Task StartLoadingProcess()
    {
        await loadSceneUIManager.ShowLoadingScreen();
        await LoadGameData();
    }
    public async Task LoadGameData()
    {
        if (AuthManager.instance?.User?.UserId == null)
        {
            Debug.LogError("User is not authenticated.");
            return;
        }

        await LoadCollection(AuthManager.instance.User.UserId);
    }



    private async Task LoadCollection(string uid)
    {
        var snapshot = await firestore.Collection($"users/{uid}/players").GetSnapshotAsync();
        if (snapshot == null || snapshot.Count == 0)
        {
            Debug.LogWarning("No players found.");
            return;
        }

        Players.Clear();
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        List<Task<GameObject>> tileTasks = new List<Task<GameObject>>();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Player data = document.ConvertTo<Player>();
            Players.Add(document.Id, data);
            tileTasks.Add(CreatePlayerTile(data));
        }

        GameObject[] tiles = await Task.WhenAll(tileTasks);
        foreach (var tile in tiles) tile.SetActive(true);

        Debug.Log($"Loaded {Players.Count} players.");
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
