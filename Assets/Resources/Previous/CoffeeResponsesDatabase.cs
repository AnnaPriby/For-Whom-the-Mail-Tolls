using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CoffeeResponsesDatabase", menuName = "Databases/CoffeeResponses")]
public class CoffeeResponsesDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries;
}