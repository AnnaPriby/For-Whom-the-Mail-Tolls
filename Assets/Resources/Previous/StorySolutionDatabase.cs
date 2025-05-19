using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorySolutionDatabase", menuName = "Email Tables/Story Solution")]
public class StorySolutionDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries = new(); // Not StorySolutionData anymore
}
