
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GreetingDatabase", menuName = "Email Tables/Greeting")]
public class GreetingDatabase : ScriptableObject
{ 
    public List<EmailData> entries = new();
}








