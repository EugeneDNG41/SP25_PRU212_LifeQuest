using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class PlayedScenario
{
    [FirestoreProperty]
    public string ScenarioDecription { get; set; }
    [FirestoreProperty]
    public string ChoiceDescription { get; set; }
    [FirestoreProperty]
    public string OutcomeDescription { get; set; }
}
