using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class StatImpact
{
    //public string ImpactId { get; set; }

    [FirestoreProperty]
    public int HealthImpact { get; set; }

    [FirestoreProperty]
    public int HappinessImpact { get; set; }

    [FirestoreProperty]
    public int WealthImpact { get; set; }
}
