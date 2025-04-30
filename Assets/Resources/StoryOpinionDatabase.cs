using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryOpinionDatabase", menuName = "Email Tables/StoryOpinion")]
public class StoryOpinionDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries = new();
}
