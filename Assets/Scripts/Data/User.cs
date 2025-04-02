using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string Username { get; set; }
    [FirestoreProperty]
    public Dictionary<string, Player> Players { get; set; } = new();
    //[FirestoreProperty]
    //public Dictionary<string, UnlockedAchievement> UnlockedAchievements { get; set; } = new();
}
