using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class AgeRange
{

    [FirestoreProperty]
    public int MinAge { get; set; }

    [FirestoreProperty]
    public int MaxAge { get; set; }
}
