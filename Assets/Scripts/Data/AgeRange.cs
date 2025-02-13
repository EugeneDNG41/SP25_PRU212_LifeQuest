using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class AgeRange
{
    //public string AgeRangeId { get; set; }

    [FirestoreProperty]
    public int MinAge { get; set; }

    [FirestoreProperty]
    public int MaxAge { get; set; }
}
