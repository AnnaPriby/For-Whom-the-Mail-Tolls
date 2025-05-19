using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoffeeEmailsDatabase", menuName = "Databases/CoffeeEmails")]
public class CoffeeEmailsDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries;
}
