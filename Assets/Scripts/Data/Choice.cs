using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Choice
{

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public string RequiredTraitId { get; set; }
    [FirestoreProperty]
    public string QuizId { get; set; }
    public Quiz Quiz { get; set; }
    public Trait RequiredTrait { get; set; }

    [FirestoreProperty]
    public Dictionary<string, Outcome> Outcomes { get; set; } = new();
}
