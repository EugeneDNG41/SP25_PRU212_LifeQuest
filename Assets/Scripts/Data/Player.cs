using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class Player
{

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public int Age { get; set; }

    [FirestoreProperty]
    public int Health { get; set; }

    [FirestoreProperty]
    public int Happiness { get; set; }

    [FirestoreProperty]
    public int Wealth { get; set; }

    [FirestoreProperty]
    public string Sex { get; set; }

    [FirestoreProperty]
    public string Status { get; set; }
    [FirestoreProperty]
    public string DeathId { get; set; }

    [FirestoreProperty]
    public string StageId { get; set; }

    [FirestoreProperty]
    public string ScenarioId { get; set; }

    [FirestoreProperty]
    public Dictionary<string, Trait> UnlockedTraits { get; set; } = new();
    [FirestoreProperty]
    public Dictionary<string, PlayedScenario> PlayedScenarios { get; set; } = new();


    public Player()
    {
        Health = 50;
        Happiness = 50;
        Wealth = 50;
        Age = 0;
        UnlockedTraits = new Dictionary<string, Trait>();
        PlayedScenarios = new Dictionary<string, PlayedScenario>();
    }
}
