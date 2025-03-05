using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Quiz
{
    //public string QuizId { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public Dictionary<string, QuizQuestion> Questions { get; set; } = new();
    [FirestoreProperty]
    public Dictionary<string, Outcome> Outcomes { get; set; } = new();
}
