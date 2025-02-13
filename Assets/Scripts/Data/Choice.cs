using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Choice
{
    //public string ChoiceId { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public string RequiredTraitId { get; set; }
    public Trait RequiredTrait { get; set; }

    [FirestoreProperty]
    public Dictionary<string, Outcome> Outcomes { get; set; } = new();
}
