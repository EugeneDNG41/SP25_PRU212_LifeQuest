using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Firestore;

[FirestoreData]
public class Scenario
{
    //public string ScenarioId { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public string RequiredTraitId { get; set; }
    public Trait RequiredTrait { get; set; }

    [FirestoreProperty]
    public string AgeRangeId { get; set; }
    public AgeRange AgeRange { get; set; }

    [FirestoreProperty]
    public Dictionary<string, Choice> Choices { get; set; } = new();
}
