using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[FirestoreData]
public class Achievement
{

    [FirestoreProperty]
    public string Name { get; set; }
    [FirestoreProperty]
    public string Description { get; set; }
}
