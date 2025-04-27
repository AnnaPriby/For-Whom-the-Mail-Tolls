using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryOpinionData
{
    public string Name;
    public List<EmailVariant> variants;
}

[CreateAssetMenu(fileName = "StoryOpinionDatabase", menuName = "Email Tables/StoryOpinion")]
public class StoryOpinionDatabase : ScriptableObject
{
    public List<StoryOpinionData> entries;
}
