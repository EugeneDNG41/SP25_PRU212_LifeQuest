using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[FirestoreData]
public class UnlockedAchievement
{
    [FirestoreProperty]
    public string AchievementId { get; set; }
    [FirestoreProperty]
    public DateTime UnlockDate { get; set; }
}

