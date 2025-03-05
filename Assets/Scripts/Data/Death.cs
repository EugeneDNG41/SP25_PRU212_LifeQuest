using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Death
{
    [FirestoreProperty]
    public string Title { get; set; }

    [FirestoreProperty]
    public string Description { get; set; }
    [FirestoreProperty]
    public string Cause { get; set; } // AGE100, HEALTH0, HAPPINESS0, WEALTH0, HAPPINESS100, WEALTH100, HEALTH100
    [FirestoreProperty]
    public string StageId { get; set; }

}

