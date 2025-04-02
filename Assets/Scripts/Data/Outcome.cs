using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Outcome
{

    [FirestoreProperty]
    public string Description { get; set; }

    [FirestoreProperty]
    public string ImpactId { get; set; }
    public StatImpact Impact { get; set; }

    [FirestoreProperty]
    public string ResultTraitId { get; set; }
    public Trait ResultTrait { get; set; }
}
